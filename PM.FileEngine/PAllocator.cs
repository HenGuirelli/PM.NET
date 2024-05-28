using PM.Common;
using PM.Core.PMemory;
using PM.FileEngine.FileFields;
using PM.FileEngine.Transactions;
using Serilog;

namespace PM.FileEngine
{
    public class PAllocator
    {
        public uint MinRegionSizeBytes { get; set; } = 8;
        public PmCSharpDefinedTypes PersistentMemory { get; }
        public PersistentBlockLayout? FirstPersistentBlockLayout => _firstPersistentBlockLayout;
        public bool HasAnyBlocks => _firstPersistentBlockLayout != null;

        PersistentBlockLayout? _firstPersistentBlockLayout;
        internal TransactionFile TransactionFile { get; }
        private uint _headerStartblocksOffset;

        public PAllocator(PmCSharpDefinedTypes persistentMemory, PmCSharpDefinedTypes transactionFile)
        {
            PersistentMemory = persistentMemory;
            TransactionFile = new TransactionFile(transactionFile, this);

            if (!IsHeaderLayoutCreated())
            {
                CreateHeaderLayout();
            }

            LoadBlockLayouts();
        }

        private void CreateHeaderLayout()
        {
            PersistentMemory.WriteUInt(OriginalFileOffsets.HeaderAssemblyNameOffset, OriginalFileOffsets.HeaderStartBlocksOffset);
            PersistentMemory.WriteUInt(OriginalFileValues.HeaderVersion, OriginalFileOffsets.HeaderVersionOffset);
            PersistentMemory.WriteByte((byte)OriginalFileValues.HeaderCommitByte, OriginalFileOffsets.HeaderCommitByte);
        }

        public bool IsHeaderLayoutCreated()
        {
            // Verify Commit byte
            return PersistentMemory.ReadByte() == (byte)CommitState.Commited;
        }

        private void LoadBlockLayouts()
        {
            // has block?
            _headerStartblocksOffset = PersistentMemory.ReadUInt(offset: OriginalFileOffsets.HeaderStartBlocksOffset);
            // Verify if "regions quantity" is equals 0.
            if (_headerStartblocksOffset < 5 || PersistentMemory.ReadByte(offset: _headerStartblocksOffset) == 0) return;

            LoadBlock(_headerStartblocksOffset);
        }

        private void LoadBlock(uint startblocksOffset)
        {
            var offset = startblocksOffset;

            PersistentBlockLayout? lastBlock = null;
            while (true)
            {
                var block = PersistentBlockLayout.LoadBlockLayoutFromPm(offset, PersistentMemory, TransactionFile);

                // First block
                if (_firstPersistentBlockLayout is null) _firstPersistentBlockLayout = block;

                // Not the first block
                if (lastBlock != null)
                {
                    lastBlock.NextBlock = block;
                    lastBlock._nextBlockOffset = block.BlockOffset;
                }

                block.LoadRegionsFromPm();

                lastBlock = block;

                if (block.NextBlockOffset == 0) break;
                offset = block.NextBlockOffset;
            }
        }

        public PersistentRegion Alloc(uint size)
        {
            var regionSize = size <= MinRegionSizeBytes ?
                MinRegionSizeBytes :
                BitwiseOperations.RoundUpPowerOfTwo(size);
            return GetFreeRegion(regionSize);
        }

        private PersistentRegion GetFreeRegion(uint regionSize)
        {
            // Get block
            var block = _firstPersistentBlockLayout;
            // Dont have any block
            if (block is null)
            {
                _firstPersistentBlockLayout = block = CreateNewBlock(regionSize);
                return block.GetFreeRegion()!;
            }

            // First fit algorithm
            while (block != null)
            {
                if (block.RegionsSize >= regionSize && regionSize > block.RegionsSize / 2)
                {
                    var region = block.GetFreeRegion();
                    if (region != null) return region; // Find a free region
                }
                block = block.NextBlock;
            }

            // Not find any region, should create one
            block = CreateNewBlock(regionSize);
            return block.GetFreeRegion()!;
        }

        private PersistentBlockLayout CreateNewBlock(uint regionSize)
        {
            // TODO: Collect metrics and create a optimal region quantity

            uint startBlockOffset = _headerStartblocksOffset;
            var lastBlock = GetLastBlock();
            if (lastBlock != null)
            {
                startBlockOffset = lastBlock.BlockOffset + lastBlock.TotalSizeBytes;
            }
            var regionQuantity = regionSize < 100 ? (byte)64 : (byte)4;

            TransactionFile.AddNewBlockLayout(
                new AddBlockLayout(
                    startBlockOffset: startBlockOffset,
                    regionsQtty: regionQuantity,
                    regionsSize: regionSize));

            var block = new PersistentBlockLayout(regionSize, regionQuantity: regionQuantity)
            {
                BlockOffset = startBlockOffset,
                TransactionFile = TransactionFile,
                PersistentMemory = PersistentMemory
            };
            block.LoadRegionsFromPm();

            if (lastBlock != null)
            {
                lastBlock.NextBlock = block;
                lastBlock.NextBlockOffset = block.BlockOffset;
            }

            return block;
        }

        internal void RemoveBlock(PersistentBlockLayout persistentBlockLayout)
        {
            var previousBlock = GetPreviousBlockLayout(persistentBlockLayout);
            if (previousBlock == null)
            {
                Log.Verbose("Persistent block {blockID} not not found previous block. Ignoring command..", persistentBlockLayout.BlockOffset);
                return; // not found, nothing to do
            }

            TransactionFile.AddRemoveBlockLayout(new RemoveBlockLayout(previousBlock.BlockOffset, previousBlock.BlockOffset));
        }

        private PersistentBlockLayout? GetPreviousBlockLayout(PersistentBlockLayout persistentBlockLayout)
        {
            var block = FirstPersistentBlockLayout;
            while (block != null)
            {
                if (block.NextBlock != null &&
                    block.NextBlock.BlockOffset == persistentBlockLayout.BlockOffset)
                {
                    return block;
                }

                block = block.NextBlock;
            }

            return null;
        }


        private PersistentBlockLayout? GetLastBlock()
        {
            if (_firstPersistentBlockLayout is null) return null;

            var block = _firstPersistentBlockLayout;
            while (block.NextBlock != null)
            {
                block = block.NextBlock;
            }
            return block;
        }

        internal void WriteBlockLayout(uint offset, PersistentBlockLayout block)
        {
            if (offset + block.TotalSizeBytes > PersistentMemory.FileBasedStream.Length)
            {
                PersistentMemory.IncreaseSize(minSize: offset + block.TotalSizeBytes);
            }

            PersistentMemory.WriteByte(block.RegionsQuantity, offset);
            offset += sizeof(byte);

            PersistentMemory.WriteUInt(block.RegionsSize, offset);
            offset += sizeof(int);

            PersistentMemory.WriteULong(block.FreeBlocks, offset);
            offset += sizeof(ulong);

            PersistentMemory.WriteUInt(block.NextBlockOffset, offset);

            Log.Debug(
                "{RegionsQuantity}|{RegionsSize}|{FreeBlocks}|{NextBlockOffset}",
                block.RegionsQuantity,
                block.RegionsSize,
                block.FreeBlocks,
                block.NextBlockOffset);
        }

        internal byte[] ReadTransactionFile()
        {
            return ReadFile(TransactionFile.FilePath);
        }

        internal byte[] ReadOriginalFile()
        {
            return ReadFile(PersistentMemory.FilePath);
        }

        internal static byte[] ReadFile(string filename)
        {
            using var stream = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            using var memstream = new MemoryStream();
            stream.BaseStream.CopyTo(memstream);
            return memstream.ToArray();
        }

        public PersistentRegion GetRegion(uint blockId, byte regionIndex)
        {
            var block = GetBlock(blockId);
            return block.GetRegion(regionIndex);
        }

        public PersistentBlockLayout GetBlock(uint blockId)
        {
            var blockLayout = _firstPersistentBlockLayout;
            while (blockLayout != null)
            {
                if (blockLayout.BlockOffset == blockId)
                {
                    return blockLayout;
                }

                blockLayout = blockLayout.NextBlock;
            }

            throw new ApplicationException($"Block not found. BlockId {blockId}");
        }
    }
}
