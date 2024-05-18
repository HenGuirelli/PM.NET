using Castle.DynamicProxy;
using PM.Core.PMemory;

namespace PM.AutomaticManager.Proxies
{
    public class PmInterceptor : IInterceptor
    {
        internal PersistentRegion PersistentRegion { get; }

        private readonly Type _targetType;
        private readonly PMemoryManager _memoryManager;
        // Internal objects cache. 
        // Property name used as index, with prefix "get_" (not used in set)
        private readonly Dictionary<string, object> _innerObjectsProxyCacheByPropertyName = new();

        public PmInterceptor(
            PersistentRegion persistentRegion,
            PMemoryManager memoryManager,
            Type targetType)
        {
            _targetType = targetType;
            _memoryManager = memoryManager;
            PersistentRegion = persistentRegion;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.GetConcreteMethod();
            string methodName = method.Name;
            if (method.IsSpecialName && methodName.StartsWith("set_"))
            {
                var value = CastleManager.GetValue(invocation);
                _memoryManager.UpdateProperty(PersistentRegion, _targetType, CastleManager.GetPropertyInfo(invocation), value);
                invocation.Proceed();
            }
            else if (method.IsSpecialName && methodName.StartsWith("get_"))
            {
                if (_innerObjectsProxyCacheByPropertyName.TryGetValue(methodName, out var proxyObject))
                {
                    invocation.Proceed();
                    invocation.ReturnValue = proxyObject;
                    return;
                }

                var value = _memoryManager.GetPropertyValue(PersistentRegion, _targetType, CastleManager.GetPropertyInfo(invocation), out var returnIsProxyObject);
                if (value is null) return;

                if (returnIsProxyObject)
                {
                    _innerObjectsProxyCacheByPropertyName[methodName] = value;
                }

                invocation.Proceed();
                invocation.ReturnValue = value;
            }
        }
    }
}
