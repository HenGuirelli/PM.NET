using Castle.DynamicProxy;
using PM.CastleHelpers;
using PM.Common;
using PM.Core;
using PM.Managers;
using System.Reflection;

namespace PM.Proxies
{
    internal class PersistentInterceptor : IPmInterceptor
    {
        public ThreadLocal<IInterceptorRedirect> TransactionInterceptorRedirect { get; } = new();
        public MemoryMappedFileBasedStream PmMemoryMappedFile { get; }
        public IInterceptorRedirect OriginalFileInterceptorRedirect { get; }
        private readonly Type _targetType;
        private readonly Dictionary<MethodInfo, PropertyInfo> _methodToPropCache = new();

        public string FilePointer { get; }
        public ulong? PmPointer { get; }

        public FileHandlerItem FileHandlerItem { get; }
        public ulong FilePointerCount
        { 
            get => FileHandlerItem.FilePointerReference; 
            set => FileHandlerItem.FilePointerReference = value; 
        }

#if DEBUG
        private static readonly Dictionary<PersistentInterceptor, int> _allPersistentInterceptor = new();   
#endif

        public PersistentInterceptor(
            PmManager pmManager,
            FileHandlerItem fileHandlerItem,
            Type targetType,
            string filePointer,
            ulong? pmPointer)
        {
            OriginalFileInterceptorRedirect = pmManager ?? throw new ArgumentNullException(nameof(pmManager));
            PmMemoryMappedFile = pmManager.PmMemoryMappedFile;
            _targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            FilePointer = filePointer ?? throw new ArgumentNullException(nameof(filePointer));
            PmPointer = pmPointer;
            FileHandlerItem = fileHandlerItem;

#if DEBUG
            _allPersistentInterceptor[this] = GetHashCode();
#endif
        }

        public void Intercept(IInvocation invocation)
        {
            string methodName = CastleManager.GetPropertyName(invocation);
            if (methodName.StartsWith("set_"))
            {
                var property = GetPropFromMethod(invocation.Method);

                if (property != null)
                {
                    var value = CastleManager.GetValue(invocation);

                    var interceptor = GetInterceptorRedirect();
                    interceptor.InsertValuePm(property, value);
                    PmMemoryMappedFile.Flush();
                    invocation.Proceed();
                }
            }
            else if (methodName.StartsWith("get_"))
            {
                var property = GetPropFromMethod(invocation.Method);
                if (property != null)
                {
                    var interceptor = GetInterceptorRedirect();
                    var value = interceptor.GetValuePm(property);
                    if (value is null) return;

                    invocation.Proceed();
                    invocation.ReturnValue = value;
                }
            }
        }

        private IInterceptorRedirect GetInterceptorRedirect()
        {
            return TransactionInterceptorRedirect.Value ?? OriginalFileInterceptorRedirect;
        }

        public PropertyInfo? GetPropFromMethod(MethodInfo method)
        {
            if (_methodToPropCache.TryGetValue(method, out var cacheResult)) return cacheResult;

            if (!method.IsSpecialName) return null;
            var result = _targetType.GetProperty(method.Name[4..]);

            if (result != null)
            {
                _methodToPropCache[method] = result;
            }

            return result;
        }
    }
}
