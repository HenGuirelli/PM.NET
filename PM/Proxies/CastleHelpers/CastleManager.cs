using Castle.DynamicProxy;
using PM.Proxies;

namespace PM.CastleHelpers
{
    internal class CastleManager
    {
        public static bool TryGetInterceptor(object obj, out PersistentInterceptor? interceptor)
        {
            interceptor = GetInterceptor(obj);
            return interceptor != null;
        }

        public static PersistentInterceptor? GetInterceptor(object obj)
        {
            if (obj is IProxyTargetAccessor proxyObj)
            {
                var interceptor =
                    (PersistentInterceptor)proxyObj
                        .GetInterceptors()
                        .Single(x => x is PersistentInterceptor);
                return interceptor;
            }
            return null;
        }

        public static string GetPropertyName(IInvocation invocation)
        {
            return
                invocation
                .GetConcreteMethod()
                .Name;
        }

        public static object GetValue(IInvocation invocation)
        {
            return invocation.Arguments[0];
        }

        public static Type GetPropertyType(IInvocation invocation)
        {
            return invocation.GetConcreteMethod().ReturnType;
        }
    }
}