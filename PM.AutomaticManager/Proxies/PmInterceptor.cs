using Castle.DynamicProxy;
using PM.Core.PMemory;
using System.Reflection;

namespace PM.AutomaticManager.Proxies
{
    public class PmInterceptor : IInterceptor
    {
        [ThreadStatic]
        static internal PersistentRegion? TransactionPersistentRegion;
        static bool TrasactionOperation => TransactionPersistentRegion != null;
        [ThreadStatic]
        static private Dictionary<string, object> _transactionInnerObjectsProxyCacheByPropertyName = new();
        internal bool IsRootObject { get; set; }

        internal PersistentRegion PersistentRegion { get; }
        internal Type TargetType { get; }
        // The FilePointerCount property starts with the value 1 because during creation
        // the generated proxy is not added to the proxy cache, only on the first Get (lazy loading). 
        // If it were 0, the garbage collector would immediately collect the proxy, erasing its persistent memory region.
        public int FilePointerCount { get; internal set; } = 1;

        private readonly PMemoryManager _memoryManager;
        // ==================TODO: MEMORY LEAK HERE ===========================
        // ================== SHOULD REMOVE FROM CACHE ===================
        // Internal objects cache. 
        // Property name used as index, with prefix "get_" (not used in set)
        private readonly Dictionary<string, object> _innerObjectsProxyCacheByPropertyName = new();

        public PmInterceptor(
            PersistentRegion persistentRegion,
            PMemoryManager memoryManager,
            Type targetType)
        {
            TargetType = targetType;
            _memoryManager = memoryManager;
            PersistentRegion = persistentRegion;
        }

        public void Intercept(IInvocation invocation)
        {
            if (TrasactionOperation && IsRootObject)
            {
                TransactionIntercept(invocation);
            }
            else
            {
                DefaultInercept(invocation);
            }
        }

        private void TransactionIntercept(IInvocation invocation)
        {
            // TODO: Refactory to reuse DefaultInercept
            var method = invocation.GetConcreteMethod();
            string methodName = method.Name;
            var methodNameWithouPrefix = GetPropertyName(method);
            if (_transactionInnerObjectsProxyCacheByPropertyName is null) _transactionInnerObjectsProxyCacheByPropertyName = new();

            if (method.IsSpecialName && methodName.StartsWith("set_"))
            {
                var value = CastleManager.GetValue(invocation);
                _memoryManager.UpdateProperty(TransactionPersistentRegion!, TargetType, CastleManager.GetPropertyInfo(invocation), value);

                if (value is null && _transactionInnerObjectsProxyCacheByPropertyName.TryGetValue(methodNameWithouPrefix, out var oldValue))
                {
                    if (CastleManager.TryGetCastleProxyInterceptor(oldValue, out var interceptor))
                        interceptor.FilePointerCount--;
                    _transactionInnerObjectsProxyCacheByPropertyName.Remove(methodNameWithouPrefix);
                }

                if (value != null && CastleManager.TryGetCastleProxyInterceptor(value, out var interceptor2))
                {
                    interceptor2.FilePointerCount++;
                }

                invocation.Proceed();
            }
            else if (method.IsSpecialName && methodName.StartsWith("get_"))
            {
                if (
                    _transactionInnerObjectsProxyCacheByPropertyName.TryGetValue(methodNameWithouPrefix, out var proxyObject) ||
                    _innerObjectsProxyCacheByPropertyName.TryGetValue(methodNameWithouPrefix, out proxyObject))
                {
                    invocation.Proceed();
                    invocation.ReturnValue = proxyObject;
                    return;
                }

                var value = _memoryManager.GetPropertyValue(TransactionPersistentRegion!, TargetType, CastleManager.GetPropertyInfo(invocation), out var returnIsProxyObject);
                if (value is null) return;

                if (returnIsProxyObject)
                {
                    _transactionInnerObjectsProxyCacheByPropertyName[methodNameWithouPrefix] = value;
                }

                invocation.Proceed();
                invocation.ReturnValue = value;
            }
        }

        private void DefaultInercept(IInvocation invocation)
        {
            var method = invocation.GetConcreteMethod();
            string methodName = method.Name;
            var methodNameWithouPrefix = GetPropertyName(method);
            if (method.IsSpecialName && methodName.StartsWith("set_"))
            {
                var value = CastleManager.GetValue(invocation);
                _memoryManager.UpdateProperty(PersistentRegion, TargetType, CastleManager.GetPropertyInfo(invocation), value);

                if (value is null && _innerObjectsProxyCacheByPropertyName.TryGetValue(methodNameWithouPrefix, out var oldValue))
                {
                    if (CastleManager.TryGetCastleProxyInterceptor(oldValue, out var interceptor))
                        interceptor.FilePointerCount--;
                    _innerObjectsProxyCacheByPropertyName.Remove(methodNameWithouPrefix);
                }

                if (value != null && CastleManager.TryGetCastleProxyInterceptor(value, out var interceptor2))
                {
                    interceptor2.FilePointerCount++;
                }

                invocation.Proceed();
            }
            else if (method.IsSpecialName && methodName.StartsWith("get_"))
            {
                if (_innerObjectsProxyCacheByPropertyName.TryGetValue(methodNameWithouPrefix, out var proxyObject))
                {
                    invocation.Proceed();
                    invocation.ReturnValue = proxyObject;
                    return;
                }

                var value = _memoryManager.GetPropertyValue(PersistentRegion, TargetType, CastleManager.GetPropertyInfo(invocation), out var returnIsProxyObject);
                if (value is null) return;

                if (returnIsProxyObject)
                {
                    _innerObjectsProxyCacheByPropertyName[methodNameWithouPrefix] = value;
                }

                invocation.Proceed();
                invocation.ReturnValue = value;
            }
        }

        public static string GetPropertyName(MethodInfo method)
        {
            // Remove the prefix "get_" or "set_" from method name
            return method.Name[4..];
        }
    }
}
