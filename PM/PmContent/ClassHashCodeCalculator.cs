namespace PM.PmContent
{
    public class ClassHashCodeCalculator
    {
        public static int GetHashCode(Type type)
        {
            int result = 0;
            // TODO: ordenar antes de calcular hash
            foreach (var item in type.GetProperties())
            {
                result += item.GetHashCode() + item.Name.GetHashCode();
            }
            return result;
        }
    }
}
