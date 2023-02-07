using Castle.DynamicProxy;

namespace PM.CastleHelpers
{
    internal class CastleManager
    {
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