﻿using PM.Common;
using PM.Core.PMemory;
using PM.FileEngine.FileFields;
using PM.FileEngine.Transactions;
using Serilog;

namespace PM.FileEngine
{
    public interface IPAllocatorAddBlock
    {
        void Addblock(PersistentBlockLayout persistentBlockLayout);
    }

    public interface IPAllocatorRemoveBlock
    {
        void Removeblock(PersistentBlockLayout persistentBlockLayout);
    }


    internal class PAllocatorEngineRemove : IPAllocatorRemoveBlock
    {
        private PmCSharpDefinedTypes _originalFile;
        private TransactionFile _transactionFile;
        private PersistentBlockLayout _firstPersistentBlockLayout;

        public PAllocatorEngineRemove(
            PmCSharpDefinedTypes originalFile,
            TransactionFile transactionFile,
            // Used to get address
            PersistentBlockLayout firstPersistentBlockLayout)
        {
            _originalFile = originalFile;
            _transactionFile = transactionFile;
            _firstPersistentBlockLayout = firstPersistentBlockLayout;
        }

        public void Removeblock(PersistentBlockLayout persistentBlockLayout)
        {
            var removedBlock = _firstPersistentBlockLayout;
            PersistentBlockLayout? beforeBlock = null;
            PersistentBlockLayout? afterBlock = null;
            while (removedBlock.NextBlock != null)
            {
                if (removedBlock == persistentBlockLayout) break;
                beforeBlock = removedBlock;
                removedBlock = removedBlock.NextBlock;
            }

            _transactionFile.AddRemoveBlockLayout(new RemoveBlockLayout(
                beforeBlockOffset: beforeBlock?.BlockOffset ?? 0,
                removedBlockOffset: persistentBlockLayout.BlockOffset,
                afterBlockOffset: afterBlock?.BlockOffset ?? 0));

            _transactionFile.ApplyPendingTransaction();
        }
    }

    internal class PAllocatorEngineAdd : IPAllocatorAddBlock
    {
        private readonly PmCSharpDefinedTypes _originalFile;
        private readonly TransactionFile _transactionFile;

        private PersistentBlockLayout _lastBlock;

        public PAllocatorEngineAdd(
            PmCSharpDefinedTypes originalFile,
            TransactionFile transactionFile,
            // Used to get address
            PersistentBlockLayout firstPersistentBlockLayout)
        {
            _originalFile = originalFile;
            _transactionFile = transactionFile;

            var block = firstPersistentBlockLayout;
            while (block.NextBlock != null) { block = block.NextBlock; }
            _lastBlock = block;
        }

        public void Addblock(PersistentBlockLayout persistentBlockLayout)
        {
            var newBlockOffset = _lastBlock.BlockOffset + _lastBlock.TotalSizeBytes;
            _transactionFile.AddNewBlockLayout(
                new AddBlockLayout(
                    startBlockOffset: newBlockOffset,
                    regionsQtty: persistentBlockLayout.RegionsQuantity,
                    regionsSize: persistentBlockLayout.RegionsSize
                ));

            // This operation modify original file
            _lastBlock.NextBlockOffset = newBlockOffset;

            _transactionFile.ApplyPendingTransaction();
        }
    }

    public class PAllocator
    {
        public uint MinRegionSizeBytes { get; set; } = 8;
        public PmCSharpDefinedTypes PersistentMemory { get; }

        private readonly TransactionFile _transactionFile;
        PersistentBlockLayout? _firstPersistentBlockLayout;
        private uint _headerStartblocksOffset;

        public PAllocator(PmCSharpDefinedTypes persistentMemory, PmCSharpDefinedTypes transactionFile)
        {
            PersistentMemory = persistentMemory;
            _transactionFile = new TransactionFile(transactionFile, this);

            if (!IsHeaderLayoutCreated())
            {
                CreateHeaderLayout();
            }

            LoadBlockLayouts();
        }

        private void CreateHeaderLayout()
        {
            PersistentMemory.WriteUInt(OriginalFileValues.HeaderStartBlocksOffset, OriginalFileOffsets.HeaderStartBlocksOffset);
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
            if (PersistentMemory.ReadByte(offset: _headerStartblocksOffset) == 0) return;

            LoadBlock(_headerStartblocksOffset);
        }

        private void LoadBlock(uint startblocksOffset)
        {
            var offset = startblocksOffset;

            PersistentBlockLayout? lastBlock = null;
            while (true)
            {
                var regionsQuantity = PersistentMemory.ReadByte(offset);
                offset += sizeof(byte);
                var regionsSize = PersistentMemory.ReadUInt(offset);
                offset += sizeof(uint);
                var freeBlocks = PersistentMemory.ReadULong(offset);
                offset += sizeof(ulong);
                var nextBlockOffset = PersistentMemory.ReadUInt(offset);
                offset += sizeof(uint);

                var block = new PersistentBlockLayout(regionsSize, regionsQuantity)
                {
                    FreeBlocks = freeBlocks,
                    NextBlockOffset = nextBlockOffset,
                };

                // First block
                if (_firstPersistentBlockLayout is null) _firstPersistentBlockLayout = block;

                // Not the first block
                if (lastBlock != null)
                {
                    lastBlock.NextBlock = block;
                    lastBlock._nextBlockOffset = block.BlockOffset;
                }

                block.LoadFromPm();

                lastBlock = block;
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
            // Have only one available block
            else if (block.NextBlock == null)
            {
                if (block.RegionsSize >= regionSize && regionSize > block.RegionsSize / 2)
                {
                    var region = block.GetFreeRegion();
                    if (region != null) return region; // Find a free region
                }
            }

            // First fit algorithm
            while (block?.NextBlock != null)
            {
                if (block.RegionsSize >= regionSize && regionSize > block.RegionsSize / 2)
                {
                    var region = block.GetFreeRegion();
                    if (region != null) return region; // Find a free region
                }
                block = block.NextBlock;
            }

            // Not find any region, should create one
            _firstPersistentBlockLayout = block = CreateNewBlock(regionSize);
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

            _transactionFile.AddNewBlockLayout(
                new AddBlockLayout(
                    startBlockOffset: startBlockOffset,
                    regionsQtty: regionQuantity,
                    regionsSize: regionSize));

            _transactionFile.ApplyPendingTransaction();

            var block = new PersistentBlockLayout(regionSize, regionQuantity: regionQuantity)
            {
                BlockOffset = startBlockOffset,                
                TransactionFile = _transactionFile,
                PersistentMemory = PersistentMemory
            };
            block.LoadFromPm();
            
            if (lastBlock != null)
            {
                lastBlock.NextBlock = block;
                lastBlock._nextBlockOffset = block.BlockOffset;
            }

            return block;
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
            return ReadFile(_transactionFile.FilePath);
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
    }
}
