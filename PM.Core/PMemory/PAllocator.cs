using Serilog;

namespace PM.Core.PMemory
{
    public class PAllocator : IPersistentAllocator, IPersistentObject, IDisposable
    {
        private readonly PmCSharpDefinedTypes _persistentMemory;
        private PersistentAllocatorLayout? _persistentBlocksLayout;
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
                offset += sizeof(int);

                Log.Debug(
                    "{RegionsQuantity}|{RegionsSize}|{FreeBlocks}|{NextBlockOffset}",
                    block.RegionsQuantity,
                    block.RegionsSize,
                    block.FreeBlocks,
                    block.NextBlockOffset);
            }

            _persistentMemory.WriteByte(1, offset: 0); // Write commit byte
            Log.Debug("Layout commit byte write. Total blocks created: {blocks}", persistentBlocksLayout.Blocks.Count());

            _persistentBlocksLayout = persistentBlocksLayout;
            _persistentBlocksLayout.Configure();
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
            if (_persistentBlocksLayout is null)
                throw new ApplicationException($"Persistent memory not configured. Please call {nameof(CreateLayout)}() method");

            PersistentRegion? region;
            do
            {
                var block = _persistentBlocksLayout.GetOrCreateBlockBySize(regionSize);
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


            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _persistentMemory?.Dispose();
        }

        public PersistentRegion GetRegion(int blockID, int regionIndex)
        {
            var block = _persistentBlocksLayout.GetBlockByID(blockID);
            return block.GetRegion(regionIndex);
        }
    }
}
