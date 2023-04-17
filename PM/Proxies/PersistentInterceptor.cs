using Castle.DynamicProxy;
using PM.CastleHelpers;
using PM.Core;
using PM.Managers;
using System.Reflection;

namespace PM.Proxies
{
    internal class PersistentInterceptor : IInterceptor
    {
        // If a transaction is running, this redirector is called instead of _defaultInterceptorRedirect
        internal AsyncLocal<IInterceptorRedirect> TransactionInterceptorRedirect { get; } = new();
        internal FileBasedStream PmMemoryMappedFile { get; }
        public IInterceptorRedirect OriginalFileInterceptorRedirect { get; }
        private readonly Type _targetType;
        private readonly Dictionary<MethodInfo, PropertyInfo> _methodToPropCache = new();

        public string FilePointer { get; }
        public ulong? PmPointer { get; }

        public PersistentInterceptor(
            PmManager pmManager,
            Type targetType,
            string filePointer,
            ulong? pmPointer)
        {
            OriginalFileInterceptorRedirect = pmManager ?? throw new ArgumentNullException(nameof(pmManager));
            PmMemoryMappedFile = pmManager.PmMemoryMappedFile;
            _targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            FilePointer = filePointer ?? throw new ArgumentNullException(nameof(filePointer));
            PmPointer = pmPointer;
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
                    if (value is null) return;

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
