using Serilog;

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
        public ulong FreeBlocks { get; internal set; }

        /// <summary>
        /// Offset of next block of persistent memory.
        /// 
        /// If zero, this is the last block and more blocks need be created.
        /// </summary>
        public int NextBlockOffset
        {
            get => _nextBlockOffset;
            set => SetNextBlockOffset(value);
        }

        private int _nextBlockOffset;

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
        /// Block offset.
        /// 
        /// Also used as a index.
        /// </summary>
        internal int BlockOffset { get; set; }

        internal PmCSharpDefinedTypes? PersistentMemory { get; set; }

        public const int Header_RegionQuantityOffset = 0;
        public const int Header_RegionSizeOffset = 1;
        public const int Header_FreeBlockBitmapOffset = 6;
        public const int Header_NextBlockOffset = 14;

        public PersistentBlockLayout(int regionSize, byte regionQuantity)
        {
            if (!BitwiseOperations.IsPowerOfTwo(regionSize)) throw new ArgumentException($"{nameof(regionSize)} must be power of two");
            if (regionQuantity <= 0) throw new ArgumentException($"{nameof(regionQuantity)} must be greater than zero");

            RegionsSize = regionSize;
            RegionsQuantity = regionQuantity;
            FreeBlocks = 0;
            Regions = new PersistentRegion[RegionsQuantity];
        }

        internal void Configure()
        {
            if (PersistentMemory is null) throw new ApplicationException($"Property {nameof(PersistentMemory)} cannot be null.");

            for (int i = 0; i < RegionsQuantity; i++)
            {
                var startPointerOffset = BlockHeaderSizeBytes + BlockOffset + (RegionsSize * i);
                var region = Regions[i] = new PersistentRegion(PersistentMemory, RegionsSize, this)
                {
                    Pointer = startPointerOffset,
                    IsFree = !BitwiseOperations.VerifyBit(FreeBlocks, i),
                    RegionIndex = i,
                };

                Log.Verbose(
                    "Region={regionID} StartPointer={startPointer} created inner block={blockID} (only in memory operation)",
                    region.RegionIndex, region.Pointer, BlockOffset);
            }
        }

        private void SetNextBlockOffset(int value)
        {
            if (PersistentMemory is null) throw new ApplicationException($"Property {nameof(PersistentMemory)} cannot be null.");

            PersistentMemory.WriteInt(value, offset: BlockOffset + Header_NextBlockOffset);
            _nextBlockOffset = value;
        }

        private int GetTotalBytes()
        {
            return BlockHeaderSizeBytes + (RegionsQuantity * RegionsSize);
        }

        /// <summary>
        /// Get next free region and mark that in use
        /// </summary>
        /// <returns>Free region or null if all regions are in use</returns>
        public PersistentRegion? GetFreeRegion()
        {
            if (PersistentMemory is null) throw new ApplicationException($"Property {nameof(PersistentMemory)} cannot be null.");

            for (uint i = 0; i < RegionsQuantity; i++)
            {
                var region = Regions[i];
                if (region.IsFree)
                {
                    FreeBlocks |= i + 1;
                    PersistentMemory.WriteULong(FreeBlocks, Header_FreeBlockBitmapOffset);
                    Log.Verbose("Update FreeBlocks value={value} for block={blockID}", FreeBlocks, BlockOffset);

                    region.IsFree = false;
                    return region;
                }
            }
            return null;
        }

        internal PersistentRegion GetRegion(int regionIndex)
        {
            return Regions[regionIndex];
        }
    }
}
