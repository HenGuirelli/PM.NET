using Castle.DynamicProxy;
using PM.Core.PMemory;
using System.Reflection;

namespace PM.AutomaticManager.Proxies
{
    internal class PmInterceptor : IInterceptor
    {
        private readonly Type _targetType;
        private readonly PMemoryManager _memoryManager;
        private readonly PersistentRegion _persistentRegion;

        public PmInterceptor(
            PersistentRegion persistentRegion,
            PMemoryManager memoryManager,
            Type targetType)
        {
            _targetType = targetType;
            _memoryManager = memoryManager;
            _persistentRegion = persistentRegion;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.GetConcreteMethod();
            string methodName = method.Name;
            if (method.IsSpecialName && methodName.StartsWith("set_"))
            {
                var property = GetPropFromMethod(invocation.Method);

                if (property != null)
                {
                    var value = CastleManager.GetValue(invocation);
                    _memoryManager.UpdateProperty(_persistentRegion, _targetType, CastleManager.GetPropertyInfo(invocation), value);
                    invocation.Proceed();
                }
            }
            else if (method.IsSpecialName && methodName.StartsWith("get_"))
            {
                var property = GetPropFromMethod(invocation.Method);
                if (property != null)
                {
                    var value = _memoryManager.GetPropertyValue(_persistentRegion, _targetType, CastleManager.GetPropertyInfo(invocation));
                    if (value is null) return;

                    invocation.Proceed();
                    invocation.ReturnValue = value;
                }
            }
        }

        private object GetPropFromMethod(MethodInfo method)
        {
            // TODO: optimize with cache
            return _targetType.GetProperty(method.Name[4..]);
        }
    }
}
