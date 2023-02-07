using System.Reflection;

namespace PM.Proxies
{
    internal interface IInterceptorRedirect
    {
        ObjectPropertiesInfoMapper ObjectMapper { get; }

        object? GetValuePm(PropertyInfo property);
        void InsertValuePm(PropertyInfo property, object value);
    }
}