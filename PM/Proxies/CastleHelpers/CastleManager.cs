using Castle.DynamicProxy;
using PM.Proxies;

namespace PM.CastleHelpers
{
    internal class CastleManager
    {
        public static bool TryGetInterceptor(object obj, out IPmInterceptor? interceptor)
        {
            interceptor = GetInterceptor(obj);
            return interceptor != null;
        }

        public static IPmInterceptor? GetInterceptor(object obj)
        {
            if (obj is IProxyTargetAccessor proxyObj)
            {
                var interceptor =
                    (IPmInterceptor)proxyObj
                        .GetInterceptors()
                        .Single(x => x is IPmInterceptor);
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