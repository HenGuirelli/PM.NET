using Serilog;

namespace PM.Core.PMemory
{
    public class PAllocator : IPersistentAllocator, IPersistentObject, IDisposable
    {
        private readonly PmCSharpDefinedTypes _persistentMemory;
        private PersistentAllocatorLayout? _persistentAllocatorLayout;
        internal PersistentAllocatorLayout? PersistentAllocatorLayout => _persistentAllocatorLayout;
        public string FilePath => _persistentMemory.FilePath;
        public int MinRegionSizeBytes { get; set; } = 8;

        public PAllocator(PmCSharpDefinedTypes persistentMemory)
        {
            _persistentMemory = persistentMemory;
        }

        public void CreateLayout(PersistentAllocatorLayout persistentBlocksLayout)
        {
            if (IsLayoutCreated())
            {
                throw new PersistentLayoutAlreadyCreated(
                    $"Persistent layout already created. " +
                    $"Try call method {nameof(IsLayoutCreated)} " +
                    $"to verify if layout already created.");
            }

            persistentBlocksLayout.PmCSharpDefinedTypes = _persistentMemory;
            if (_persistentMemory.FileBasedStream.Length < persistentBlocksLayout.TotalSizeBytes)
            {
                _persistentMemory.Resize(persistentBlocksLayout.TotalSizeBytes);
            }

            _persistentAllocatorLayout = persistentBlocksLayout;
            _persistentAllocatorLayout.Configure();

            Log.Debug("Creating persistent memory layout (next lines layout 'RegionsQuantity|RegionsSize|FreeBlocks|NextBlockOffset')");
            var offset = 1; // Skip first byte (commit byte)
            foreach (var block in persistentBlocksLayout.Blocks)
            {
                _persistentMemory.WriteByte(block.RegionsQuantity, offset);
                offset += sizeof(byte);

                _persistentMemory.WriteInt(block.RegionsSize, offset);
                offset += sizeof(int);

                _persistentMemory.WriteULong(block.FreeBlocks, offset);
                offset += sizeof(ulong);

                _persistentMemory.WriteInt(block.NextBlockOffset, offset);
                offset = block.NextBlockOffset;

                Log.Debug(
                    "{RegionsQuantity}|{RegionsSize}|{FreeBlocks}|{NextBlockOffset}",
                    block.RegionsQuantity,
                    block.RegionsSize,
                    block.FreeBlocks,
                    block.NextBlockOffset);
            }

            _persistentMemory.WriteByte(1, offset: 0); // Write commit byte
            Log.Debug("Layout commit byte write. Total blocks created: {blocks}", persistentBlocksLayout.Blocks.Count());
        }

        public bool IsLayoutCreated()
        {
            // Verify Commit byte
            return _persistentMemory.ReadByte() == 1;
        }

        public PersistentRegion Alloc(int size)
        {
            var regionSize = size <= MinRegionSizeBytes ?
                MinRegionSizeBytes :
                BitwiseOperations.RoundUpPowerOfTwo(size);
            return GetFreeRegion(regionSize);
        }

        private PersistentRegion GetFreeRegion(int regionSize)
        {
            if (_persistentAllocatorLayout is null)
                throw new ApplicationException($"Persistent memory not configured. Please call {nameof(CreateLayout)}() method");

            PersistentRegion? region;
            do
            {
                var block = _persistentAllocatorLayout.GetOrCreateBlockBySize(regionSize);
                region = block.GetFreeRegion();
            } while (region is null);
            return region;
        }

        public void Free(nint pointer)
        {

        }

        public void Load()
        {
            // Verify Commit byte
            if (!IsLayoutCreated())
            {
                // Create empty layout
                CreateLayout(new PersistentAllocatorLayout());
                return;
            }

            // Skip first byte (commit byte)
            var offset = 1;
            _persistentAllocatorLayout = new PersistentAllocatorLayout
            {
                PmCSharpDefinedTypes = _persistentMemory
            };
            bool lastBlock;
            do
            {
                var blockOffset = offset;
                var regionQuantity = _persistentMemory.ReadByte(offset);
                offset += 1;
                var regionSize = _persistentMemory.ReadInt(offset);
                offset += 4;
                var freeBlocksBitmap = _persistentMemory.ReadULong(offset);
                offset += 8;
                var nextBlockOffset = _persistentMemory.ReadInt(offset);
                offset += 4;

                var block = new PersistentBlockLayout(regionSize, regionQuantity)
                {
                    PersistentMemory = _persistentMemory,
                    FreeBlocks = freeBlocksBitmap,
                    BlockOffset = blockOffset,
                    _nextBlockOffset = nextBlockOffset,
                };
                _persistentAllocatorLayout.AddLoadedBlock(block, offset);

                offset = nextBlockOffset;
                lastBlock = nextBlockOffset == 0;
            } while (!lastBlock);

            _persistentAllocatorLayout.Configure();
        }

        public void Dispose()
        {
            _persistentMemory?.Dispose();
        }

        public PersistentRegion GetRegion(int blockID, int regionIndex)
        {
            var block = _persistentAllocatorLayout.GetBlockByID(blockID);
            return block.GetRegion(regionIndex);
        }
    }
}
