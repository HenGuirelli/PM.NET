using Castle.DynamicProxy;
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
            // Remove o prefixo "get_" ou "set_" do nome do m�todo
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
    }
}