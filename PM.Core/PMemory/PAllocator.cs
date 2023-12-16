using System.IO;

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
        public nint Pointer { get; set; }

        /// <summary>
        /// Region size in bytes
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Data of region
        /// </summary>
        public byte[] Data { get; }

        private readonly PmCSharpDefinedTypes _persistentMemory;

        public PersistentRegion(PmCSharpDefinedTypes persistentMemory)
        {
            _persistentMemory = persistentMemory;
        }
    }

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
        /// 
        /// For example, if only the last region is in use,
        /// then the bits will be ...00001 and the value 1.
        /// </summary>
        public ulong FreeBlocks { get; internal set; }

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
        /// Block offset
        /// </summary>
        internal int BlockOffset { get; set; }

        internal PmCSharpDefinedTypes? PersistentMemory { get; set; }

        public PersistentBlockLayout(int regionSize, byte regionQuantity)
        {
            if (!IsPowerOfTwo(regionSize)) throw new ArgumentException($"{nameof(regionSize)} need be power of two");
            if (regionQuantity <= 0) throw new ArgumentException($"{nameof(regionQuantity)} need be greater than zero");

            RegionsSize = regionSize;
            RegionsQuantity = regionQuantity;
            Regions = new PersistentRegion[RegionsSize];
        }

        private static bool IsPowerOfTwo(int number)
        {
            return number > 0 && (number & (number - 1)) == 0;
        }

        private int GetTotalBytes()
        {
            // 17 =
            //  1 byte (block region quantity) +
            //  4 bytes (Region size) +
            //  8 bytes (Free blocks) +
            //  4 bytes (Next block layout start offset)
            return 17 + (RegionsQuantity * RegionsSize);
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
                    return region;
                }
            }
            return null;
        }
    }

    public class PersistentAllocatorLayout
    {
        private readonly Dictionary<int, PersistentBlockLayout> _blocks = new();
        public IEnumerable<PersistentBlockLayout> Blocks => _blocks.Values.ToList().AsReadOnly();

        private PmCSharpDefinedTypes? _pmCSharpDefinedTypes;
        public PmCSharpDefinedTypes? PmCSharpDefinedTypes
        {
            get => _pmCSharpDefinedTypes;
            set
            {
                _pmCSharpDefinedTypes = value;
                foreach (PersistentBlockLayout block in _blocks.Values)
                {
                    block.PersistentMemory = value;
                }
            }
        }

        public int TotalSizeBytes => _blocks.Sum(x => x.Value.TotalSizeBytes);

        // Start with 1 to skip the commit byte
        private int _blocksOffset = 1;

        public void AddBlock(PersistentBlockLayout persistentBlockLayout)
        {
            _blocks.Add(persistentBlockLayout.RegionsSize, persistentBlockLayout);

            persistentBlockLayout.BlockOffset = _blocksOffset;
            _blocksOffset += persistentBlockLayout.TotalSizeBytes;
        }

        public PersistentBlockLayout GetBlockBySize(int size)
        {
            if (_blocks.TryGetValue(size, out var block))
            {
                return block;
            }

            // TODO: Create block with new sizes
            throw new ApplicationException($"Block with size {size} not found");
        }
    }

    public class PAllocator : IPersistentAllocator, IPersistentObject, IDisposable
    {
        private readonly PmCSharpDefinedTypes _persistentMemory;
        private readonly PersistentAllocatorLayout _persistentBlocksLayout;
        public string FilePath => _persistentMemory.FilePath;
        public const int MinRegionSizeBytes = 8;

        public PAllocator(
            PersistentAllocatorLayout persistentRegionLayout,
            PmCSharpDefinedTypes persistentMemory)
        {
            _persistentMemory = persistentMemory;
            _persistentBlocksLayout = persistentRegionLayout;
            _persistentBlocksLayout.PmCSharpDefinedTypes = persistentMemory;

            CreateLayout();
        }

        private void CreateLayout()
        {
            if (_persistentMemory.FileBasedStream.Length < _persistentBlocksLayout.TotalSizeBytes)
            {
                _persistentMemory.Resize(_persistentBlocksLayout.TotalSizeBytes);
            }

            if (IsSetCommitByte())
            {
                // Layout already created.
                return;
            }

            var offset = 1; // Skip first byte (commit byte)
            foreach (var block in _persistentBlocksLayout.Blocks)
            {
                _persistentMemory.WriteByte(block.RegionsQuantity, offset);
                offset += sizeof(byte);

                _persistentMemory.WriteInt(block.RegionsSize, offset);
                offset += sizeof(int);

                _persistentMemory.WriteULong(block.FreeBlocks, offset);
                offset += sizeof(ulong);

                _persistentMemory.WriteInt(block.NextBlockOffset, offset);
                offset += sizeof(int);
            }

            _persistentMemory.WriteByte(1, offset: 0);
        }

        private bool IsSetCommitByte()
        {
            return _persistentMemory.ReadByte() == 1;
        }

        public PersistentRegion Alloc(int size)
        {
            var regionSize = RoundUpPow2(size);
            var region = GetFreeRegion(regionSize);
            return region;
        }

        private PersistentRegion GetFreeRegion(int regionSize)
        {
            PersistentRegion? region;
            do
            {
                var block = _persistentBlocksLayout.GetBlockBySize(regionSize);
                region = block.GetFreeRegion();
            } while (region is null);
            return region;
        }

        public static int RoundUpPow2(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{nameof(value)} must be greater than zero");
            }

            if (value <= MinRegionSizeBytes) return MinRegionSizeBytes;

            // number already is power of 2
            if ((value & (value - 1)) == 0) return value;

            // Find the most significant bit and increment
            int moreSignificantbit = 1;
            while (moreSignificantbit < value)
            {
                moreSignificantbit <<= 1;
            }

            return moreSignificantbit;
        }

        public void Free(nint pointer)
        {

        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _persistentMemory?.Dispose();
        }
    }
}
