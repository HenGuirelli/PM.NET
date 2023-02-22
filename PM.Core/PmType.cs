namespace PM.Core
{
    public class PmType
    {
        public Type Type { get; }
        public int SizeBits { get; }
        public int SizeBytes { get; }
        public int ID { get; }

        public PmType(Type type, int id, int sizeBits)
        {
            Type = type;
            SizeBits = sizeBits;
            SizeBytes = sizeBits / 8;
            ID = id;
        }
    }
}
