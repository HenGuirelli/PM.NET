namespace PM.Core.PMemory
{
    /// <summary>
    /// Region of persistent memory.
    /// </summary>
    public class PersistentRegion
    {
        /// <summary>
        /// Define the region is free or in use.
        /// </summary>
        internal bool IsFree { get; set; }

        /// <summary>
        /// Start pointer to region
        /// </summary>
        public int Pointer { get; set; }

        /// <summary>
        /// Region size in bytes
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Index of region inside a block
        /// </summary>
        internal int RegionIndex { get; set; }

        internal PmCSharpDefinedTypes? PersistentMemory { get; set; }

        public PersistentRegion(int size)
        {
            Size = size;
        }

        public byte[] GetData(int count, int offset)
        {
            if (offset >= Size) throw new ArgumentOutOfRangeException($"{nameof(offset)} must be less than {Size}");

            return PersistentMemory.ReadBytes(count, offset + (int)Pointer);
        }
    }
}
