//using PM.Common;
//using Serilog;

//namespace PM.Core.PMemory
//{
//    public class PAllocator : IPersistentAllocator, IPersistentObject, IDisposable
//    {
//        internal readonly PmCSharpDefinedTypes PersistentMemory;
//        private readonly TransactionFile _transactionFile;
//        private PersistentAllocatorLayout? _persistentAllocatorLayout;
//        internal PersistentAllocatorLayout? PersistentAllocatorLayout => _persistentAllocatorLayout;
//        public string FilePath => PersistentMemory.FilePath;
//        public int MinRegionSizeBytes { get; set; } = 8;

//        public PAllocator(PmCSharpDefinedTypes persistentMemory, PmCSharpDefinedTypes transactionFile)
//        {
//            PersistentMemory = persistentMemory;
//            _transactionFile = new TransactionFile(transactionFile, this);
//        }

//        public void CreateLayout(PersistentAllocatorLayout persistentBlocksLayout)
//        {
//            if (IsLayoutCreated())
//            {
//                throw new PersistentLayoutAlreadyCreated(
//                    $"Persistent layout already created. " +
//                    $"Try call method {nameof(IsLayoutCreated)} " +
//                    $"to verify if layout already created.");
//            }

//            persistentBlocksLayout.PmCSharpDefinedTypes = PersistentMemory;
//            if (PersistentMemory.FileBasedStream.Length < persistentBlocksLayout.TotalSizeBytes)
//            {
//                PersistentMemory.Resize(persistentBlocksLayout.TotalSizeBytes);
//            }

//            _persistentAllocatorLayout = persistentBlocksLayout;
//            _persistentAllocatorLayout.Configure();

//            Log.Debug("Creating persistent memory layout (next lines layout 'RegionsQuantity|RegionsSize|FreeBlocks|NextBlockOffset')");
//            if (!persistentBlocksLayout.Blocks.Any())
//            {
//                Log.Debug("No layout created.");
//            }
//            foreach (var block in persistentBlocksLayout.Blocks)
//            {
//                block.WriteBlockLayoutOnPm();
//            }

//            PersistentMemory.WriteByte(1, offset: 0); // Write commit byte
//            Log.Debug("Layout commit byte write. Total blocks created: {blocks}", persistentBlocksLayout.Blocks.Count());
//        }

//        public bool IsLayoutCreated()
//        {
//            // Verify Commit byte
//            return PersistentMemory.ReadByte() == 1;
//        }

//        /// <summary>
//        /// Write a new block layout on PMemory.
//        /// </summary>
//        /// <param name="offset">Block offset</param>
//        /// <param name="block">Block layout</param>
//        internal void WriteBlockLayout(uint offset, PersistentBlockLayout block)
//        {
//            PersistentMemory.WriteByte(block.RegionsQuantity, offset);
//            offset += sizeof(byte);

//            PersistentMemory.WriteInt(block.RegionsSize, offset);
//            offset += sizeof(int);

//            PersistentMemory.WriteULong(block.FreeBlocks, offset);
//            offset += sizeof(ulong);

//            PersistentMemory.WriteUInt(block.NextBlockOffset, offset);

//            Log.Debug(
//                "{RegionsQuantity}|{RegionsSize}|{FreeBlocks}|{NextBlockOffset}",
//                block.RegionsQuantity,
//                block.RegionsSize,
//                block.FreeBlocks,
//                block.NextBlockOffset);
//        }

//        public PersistentRegion Alloc(int size)
//        {
//            var regionSize = size <= MinRegionSizeBytes ?
//                MinRegionSizeBytes :
//                BitwiseOperations.RoundUpPowerOfTwo(size);
//            return GetFreeRegion(regionSize);
//        }

//        private PersistentRegion GetFreeRegion(int regionSize)
//        {
//            if (_persistentAllocatorLayout is null)
//                throw new ApplicationException($"Persistent memory not configured. Please call {nameof(CreateLayout)}() method");

//            PersistentRegion? region;
//            do
//            {
//                var block = _persistentAllocatorLayout.GetOrCreateBlockBySize(regionSize);
//                region = block.GetFreeRegion();
//            } while (region is null);
//            return region;
//        }

//        public void Free(nint pointer)
//        {

//        }

//        public void Load()
//        {
//            // Verify Commit byte
//            if (!IsLayoutCreated())
//            {
//                // Create empty layout
//                CreateLayout(new PersistentAllocatorLayout());
//                return;
//            }

//            // Skip first byte (commit byte)
//            var offset = 1u;
//            _persistentAllocatorLayout = new PersistentAllocatorLayout
//            {
//                PmCSharpDefinedTypes = PersistentMemory
//            };
//            bool lastBlock;
//            do
//            {
//                var blockOffset = offset;
//                var regionQuantity = PersistentMemory.ReadByte(offset);
//                offset += 1;
//                var regionSize = PersistentMemory.ReadInt(offset);
//                offset += 4;
//                var freeBlocksBitmap = PersistentMemory.ReadULong(offset);
//                offset += 8;
//                var nextBlockOffset = PersistentMemory.ReadUInt(offset);
//                offset += 4;

//                var block = new PersistentBlockLayout(regionSize, regionQuantity)
//                {
//                    PersistentMemory = PersistentMemory,
//                    FreeBlocks = freeBlocksBitmap,
//                    BlockOffset = blockOffset,
//                    _nextBlockOffset = nextBlockOffset,
//                };
//                _persistentAllocatorLayout.AddLoadedBlock(block, offset);

//                offset = nextBlockOffset;
//                lastBlock = nextBlockOffset == 0;
//            } while (!lastBlock);

//            _persistentAllocatorLayout.Configure();
//        }

//        public void Dispose()
//        {
//            PersistentMemory?.Dispose();
//        }

//        public PersistentRegion GetRegion(uint blockID, int regionIndex)
//        {
//            var block = _persistentAllocatorLayout.GetBlockByID(blockID);
//            return block.GetRegion(regionIndex);
//        }
//    }
//}
