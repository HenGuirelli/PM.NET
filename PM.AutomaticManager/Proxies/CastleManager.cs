using Castle.DynamicProxy;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PM.AutomaticManager.Proxies
{
    public class CastleManager
    {
        public static PropertyInfo GetPropertyInfo(IInvocation invocation)
        {
            MethodInfo method = invocation.GetConcreteMethod();
            return method.DeclaringType?.GetProperty(GetPropertyName(method))
                    ?? throw new ArgumentException("Inocation is not a property");
        }

        private static string GetPropertyName(MethodInfo method)
        {
            // Remove the prefix "get_" or "set_" from method name
            return method.Name[4..];
        }

        public static object GetValue(IInvocation invocation)
        {
            return invocation.Arguments[0];
        }

        public static Type GetPropertyReturnType(IInvocation invocation)
        {
            return invocation.GetConcreteMethod().ReturnType;
        }

        public static bool TryGetCastleProxyInterceptor(object? obj, [NotNullWhen(true)] out PmInterceptor? pmInterceptor)
        {
            pmInterceptor = null;
            if (obj == null)
            {
                return false;
            }

            if (obj is IProxyTargetAccessor proxyTargetAccessor)
            {
                // A proxy object must have only one interceptor of type PmInterceptor
                var interceptor = (PmInterceptor?)proxyTargetAccessor.GetInterceptors().FirstOrDefault(x => x is PmInterceptor);
                pmInterceptor = interceptor;
                return pmInterceptor != null;
            }

            return false;
        }
    }
}