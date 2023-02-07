namespace PM.PmContent
{
    public class PmHeader
    {
        public int HeaderSize => sizeof(int);
        public int ClassHash { get; }
        public int ClassHashOffset => 0;

        public PmHeader(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            ClassHash = ClassHashCodeCalculator.GetHashCode(type);
        }
    }
}
