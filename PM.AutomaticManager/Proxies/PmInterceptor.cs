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
        static private readonly Dictionary<string, object> _transactionInnerObjectsProxyCacheByPropertyName = new();
        internal bool IsRootObject { get; set; }

        internal PersistentRegion PersistentRegion { get; }
        internal Type TargetType { get; }

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
            if (method.IsSpecialName && methodName.StartsWith("set_"))
            {
                var value = CastleManager.GetValue(invocation);
                _memoryManager.UpdateProperty(TransactionPersistentRegion!, TargetType, CastleManager.GetPropertyInfo(invocation), value);

                // Set property as null. Remove from cache.
                if (value is null && _transactionInnerObjectsProxyCacheByPropertyName.ContainsKey(methodNameWithouPrefix))
                {
                    _transactionInnerObjectsProxyCacheByPropertyName.Remove(methodNameWithouPrefix);
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

                // Set property as null. Remove from cache.
                if (value is null && _innerObjectsProxyCacheByPropertyName.ContainsKey(methodNameWithouPrefix))
                {
                    _innerObjectsProxyCacheByPropertyName.Remove(methodNameWithouPrefix);
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
