namespace PM.Core.PMemory
{
    /// <summary>
    /// Region of persistent memory.
    /// </summary>
    internal interface IPersistentRegion
    {
        /// <summary>
        /// Define the region is free or in use.
        /// </summary>
        bool IsFree { get; set; }

        /// <summary>
        /// Data of region
        /// </summary>
        byte[] Data { get; }
    }

    /// <summary>
    /// Represents a block of persistent memory allocated.
    /// Each block maybe have multiples regions of free or used memory.
    /// </summary>
    public interface IPersistentBlockLayout
    {
        /// <summary>
        /// Size of each region inside a block (always power of two).
        /// </summary>
        int RegionsSize { get; }

        /// <summary>
        /// Quantity of regions inside a block.
        /// </summary>
        byte RegionsQuantity { get; }

        /// <summary>
        /// BitMap of free regions inside a block.
        /// 
        /// For example, if only the last region is in use,
        /// then the bits will be ...00001 and the value 1.
        /// </summary>
        ulong FreeBlocks { get; }

        /// <summary>
        /// Offset of next block of persistent memory.
        /// 
        /// If zero, this is the last block and more blocks need be created.
        /// </summary>
        int NextBlockOffset { get; }

        /// <summary>
        /// Get block total size.
        /// </summary>
        int TotalSizeBytes { get; }
    }

    public class PersistentBlockLayout : IPersistentBlockLayout
    {
        public int RegionsSize { get; internal set; }

        public byte RegionsQuantity { get; internal set; }

        public ulong FreeBlocks { get; internal set; }

        public int NextBlockOffset { get; internal set; }

        public int TotalSizeBytes => GetTotalBytes();

        internal int BlockOffset { get; set; }

        public PersistentBlockLayout(int regionSize, byte regionQuantity)
        {
            if (!IsPowerOfTwo(regionSize)) throw new ArgumentException($"{nameof(regionSize)} need be power of two");
            if (regionQuantity <= 0) throw new ArgumentException($"{nameof(regionQuantity)} need be greater than zero");

            RegionsSize = regionSize;
            RegionsQuantity = regionQuantity;
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
    }

    /// <summary>
    /// Initial layout of persistent memory pool.
    /// 
    /// TODO: Determine the best layout
    /// </summary>
    public interface IInitialPersistentRegionLayout
    {
        IEnumerable<IPersistentBlockLayout> Blocks { get; }
        int TotalSizeBytes { get; }
    }

    public class PersistentAllocatorHeader : IInitialPersistentRegionLayout
    {
        private readonly List<IPersistentBlockLayout> _blocks = new();
        public IEnumerable<IPersistentBlockLayout> Blocks => _blocks.AsReadOnly();

        public int TotalSizeBytes => _blocks.Sum(x => x.TotalSizeBytes);

        public void AddBlock(IPersistentBlockLayout persistentBlockLayout)
        {
            _blocks.Add(persistentBlockLayout);
        }
    }

    public class PAllocator : IPersistentAllocator, IPersistentObject
    {
        private readonly PmCSharpDefinedTypes _persistentMemory;
        private readonly IInitialPersistentRegionLayout _initialPersistentBlocksLayout;

        public PAllocator(
            IInitialPersistentRegionLayout initialPersistentRegionLayout,
            PmCSharpDefinedTypes persistentMemory)
        {
            _persistentMemory = persistentMemory;
            _initialPersistentBlocksLayout = initialPersistentRegionLayout;

            CreateLayout();
        }

        private void CreateLayout()
        {
            if (_persistentMemory.FileBasedStream.Length < _initialPersistentBlocksLayout.TotalSizeBytes)
            {
                _persistentMemory.Resize(_initialPersistentBlocksLayout.TotalSizeBytes);
            }

            if (IsSetCommitByte())
            {
                // Layout already created.
                return;
            }

            var offset = 1; // Skip first byte (commit byte)
            foreach (var block in _initialPersistentBlocksLayout.Blocks)
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
        }

        private bool IsSetCommitByte()
        {
            return _persistentMemory.ReadByte() == 1;
        }

        public void Alloc(long size)
        {

        }

        public void Free(IntPtr pointer)
        {

        }

        public void Load()
        {
            throw new NotImplementedException();
        }
    }
}
