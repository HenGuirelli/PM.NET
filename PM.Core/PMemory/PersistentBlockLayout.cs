namespace PM.Core.PMemory
{
    /// <summary>
    /// Represents a block of persistent memory allocated.
    /// Each block maybe have multiples regions of free or used memory.
    /// </summary>
    public class PersistentBlockLayout
    {
        /// <summary>
        /// Size of each region inside a block (always power of two).
        /// </summary>
        public int RegionsSize { get; internal set; }

        /// <summary>
        /// Quantity of regions inside a block.
        /// </summary>
        public byte RegionsQuantity { get; internal set; }

        /// <summary>
        /// Regions inside a block
        /// </summary>
        public PersistentRegion[] Regions { get; }

        /// <summary>
        /// BitMap of free regions inside a block.
        /// </summary>
        public byte[] FreeBlocks { get; internal set; }

        /// <summary>
        /// Offset of next block of persistent memory.
        /// 
        /// If zero, this is the last block and more blocks need be created.
        /// </summary>
        public int NextBlockOffset { get; internal set; }

        /// <summary>
        /// Get block total size.
        /// </summary>
        public int TotalSizeBytes => GetTotalBytes();

        /// <summary>
        /// 17 =
        ///  1 byte (block region quantity) +
        ///  4 bytes (Region size) +
        ///  8 bytes (Free blocks) +
        ///  4 bytes (Next block layout start offset)
        /// </summary>
        public const int BlockHeaderSizeBytes = 17;

        /// <summary>
        /// Block offset
        /// </summary>
        internal int BlockOffset { get; set; }

        internal PmCSharpDefinedTypes? PersistentMemory { get; set; }

        public PersistentBlockLayout(int regionSize, byte regionQuantity)
        {
            if (!BitwiseOperations.IsPowerOfTwo(regionSize)) throw new ArgumentException($"{nameof(regionSize)} must be power of two");
            if (regionQuantity <= 0) throw new ArgumentException($"{nameof(regionQuantity)} must be greater than zero");

            RegionsSize = regionSize;
            RegionsQuantity = regionQuantity;
            FreeBlocks = new byte[(RegionsSize * RegionsQuantity) / 8];
            Regions = new PersistentRegion[RegionsSize];
        }

        public const int Header_RegionQuantityOffset = 0;
        public const int Header_RegionSizeOffset = 1;
        public const int Header_FreeBlockBitmapOffset = 5;
        public const int Header_NextBlockOffset = 13;

        internal void Configure()
        {
            var pm = PersistentMemory ?? throw new ApplicationException($"Property {nameof(PersistentMemory)} cannot be null.");

            var regionsQuantity = pm.ReadByte(Header_RegionQuantityOffset);
            var regionSize = pm.ReadUInt(Header_RegionSizeOffset);
            var freeBlocks = pm.ReadULong(Header_FreeBlockBitmapOffset);
            var nextBlockOffset = pm.ReadULong(Header_NextBlockOffset);


            for (int i = 0; i < RegionsSize; i++)
            {
                Regions[i] = new PersistentRegion(RegionsSize)
                {
                    Pointer = BlockHeaderSizeBytes + (BlockOffset * (i + 1)),
                    PersistentMemory = PersistentMemory,
                    IsFree = !BitwiseOperations.VerifyBit(FreeBlocks, i),
                    RegionIndex = i,
                };
            }
        }

        private int GetTotalBytes()
        {
            return BlockHeaderSizeBytes + (RegionsQuantity * RegionsSize);
        }

        /// <summary>
        /// Get next free region
        /// </summary>
        /// <returns>Free region or null if all regions are in use</returns>
        public PersistentRegion? GetFreeRegion()
        {
            foreach (var region in Regions)
            {
                if (region.IsFree)
                {
                    region.IsFree = false;
                    return region;
                }
            }
            return null;
        }
    }
}
