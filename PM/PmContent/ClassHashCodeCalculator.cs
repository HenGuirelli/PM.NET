using System.Reflection;

namespace PM.PmContent
{
    public class ClassHashCodeCalculator
    {
        public static int GetHashCode(Type type)
        {
            HashCode hash = new HashCode();

            hash.Add(type.FullName);

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                hash.Add(property.Name);
            }

            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (MethodInfo method in methods)
            {
                hash.Add(method.Name);
            }

            return hash.ToHashCode();
        }
    }
}
