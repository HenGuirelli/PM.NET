﻿using PM.Common;
using PM.FileEngine.Transactions;
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
        public uint RegionsSize { get; internal set; }

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
        public uint NextBlockOffset
        {
            get => _nextBlockOffset;
            set => SetNextBlockOffset(value);
        }

        internal uint _nextBlockOffset;

        /// <summary>
        /// Get block total size.
        /// </summary>
        public uint TotalSizeBytes => GetTotalBytes();

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
        public uint BlockOffset { get; internal set; }

        internal PmCSharpDefinedTypes? PersistentMemory { get; set; }
        internal TransactionFile? TransactionFile { get; set; }

        internal PersistentBlockLayout? NextBlock { get; set; }

        private int _totalRegionsInUse;
        internal bool IsFull => _totalRegionsInUse == RegionsQuantity;


        public const int Header_RegionQuantityOffset = 0;
        public const int Header_RegionSizeOffset = 0;
        public const int Header_FreeBlockBitmapOffset = 5;
        public const int Header_NextBlockOffset = 13;

        public PersistentBlockLayout(uint regionSize, byte regionQuantity)
        {
            if (!BitwiseOperations.IsPowerOfTwo(regionSize)) throw new ArgumentException($"{nameof(regionSize)} must be power of two");
            if (regionQuantity <= 0) throw new ArgumentException($"{nameof(regionQuantity)} must be greater than zero");
            // Max region count inside a block must be 64
            // because freeBlocks bitmap is a ulong (64 bits, one bit represent one region inside a block)
            if (regionQuantity > 64) throw new ArgumentException($"{nameof(regionQuantity)} must be smaller than 64");

            RegionsSize = regionSize;
            RegionsQuantity = regionQuantity;
            _totalRegionsInUse = 0;
            FreeBlocks = 0;
            Regions = new PersistentRegion[RegionsQuantity];
        }

        internal void LoadRegionsFromPm()
        {
            if (PersistentMemory is null) throw new ApplicationException($"Property {nameof(PersistentMemory)} cannot be null.");
            if (TransactionFile is null) throw new ApplicationException($"Property {nameof(TransactionFile)} cannot be null.");

            for (byte i = 0; i < RegionsQuantity; i++)
            {
                var startPointerOffset = (uint)(BlockHeaderSizeBytes + BlockOffset + (RegionsSize * i));

                var region = Regions[i] = new PersistentRegion(PersistentMemory, TransactionFile, RegionsSize, this)
                {
                    Pointer = startPointerOffset,
                    IsFree = !BitwiseOperations.IsBitOn(FreeBlocks, i),
                    RegionIndex = i,
                };

                if (!region.IsFree) _totalRegionsInUse++;

                Log.Verbose(
                    "Region={regionID} StartPointer={startPointer} created inner block={blockID} (only in memory operation)",
                    region.RegionIndex, region.Pointer, BlockOffset);
            }

            if (NextBlock != null) NextBlockOffset = NextBlock.BlockOffset;
        }

        public static PersistentBlockLayout LoadBlockLayoutFromPm(uint offset, PmCSharpDefinedTypes persistentMemory, TransactionFile transactionFile)
        {
            uint blockOffset = offset;
            var regionsQuantity = persistentMemory.ReadByte(offset);
            offset += sizeof(byte);
            var regionsSize = persistentMemory.ReadUInt(offset);
            offset += sizeof(uint);
            var freeBlocks = persistentMemory.ReadULong(offset);
            offset += sizeof(ulong);
            var nextBlockOffset = persistentMemory.ReadUInt(offset);

            return new PersistentBlockLayout(regionsSize, regionsQuantity)
            {
                FreeBlocks = freeBlocks,
                _nextBlockOffset = nextBlockOffset,
                PersistentMemory = persistentMemory,
                TransactionFile = transactionFile,
                BlockOffset = blockOffset
            };
        }

        private void SetNextBlockOffset(uint value)
        {
            if (PersistentMemory is null) throw new ApplicationException($"Property {nameof(PersistentMemory)} cannot be null.");

            PersistentMemory.WriteUInt(value, offset: BlockOffset + Header_NextBlockOffset);
            _nextBlockOffset = value;
        }

        private uint GetTotalBytes()
        {
            return (uint)(BlockHeaderSizeBytes + (RegionsQuantity * RegionsSize));
        }

        /// <summary>
        /// Get next free region and mark that in use
        /// </summary>
        /// <returns>Free region or null if all regions are in use</returns>
        public PersistentRegion? GetFreeRegion()
        {
            for (int i = 0; i < RegionsQuantity; i++)
            {
                var region = Regions[i];
                if (region.IsFree)
                {
                    FreeBlocks = FreeBlocks | (1ul << i);
                    UpdateFreeBlocks();

                    region.IsFree = false;
                    _totalRegionsInUse++;
                    return region;
                }
            }
            return null;
        }

        internal void UpdateFreeBlocks()
        {
            if (TransactionFile is null) throw new ApplicationException($"Property {nameof(TransactionFile)} cannot be null.");

            TransactionFile.UpdateFreeBlocksLayout(new UpdateFreeBlocksFromBlockLayout(BlockOffset + Header_FreeBlockBitmapOffset, FreeBlocks));
            Log.Verbose("Update FreeBlocks value={value} for block={blockID}", FreeBlocks, BlockOffset);
        }

        internal PersistentRegion GetRegion(int regionIndex)
        {
            return Regions[regionIndex];
        }


        /// <summary>
        /// Write a new block layout on PMemory.
        /// </summary>
        /// <param name="offset">Block offset</param>
        /// <param name="block">Block layout</param>
        internal void WriteBlockLayoutOnPm()
        {
            if (PersistentMemory is null) throw new ApplicationException($"{nameof(PersistentMemory)} cannot be null");

            var offset = BlockOffset;

            PersistentMemory.WriteByte(RegionsQuantity, offset);
            offset += sizeof(byte);

            PersistentMemory.WriteUInt(RegionsSize, offset);
            offset += sizeof(uint);

            PersistentMemory.WriteULong(FreeBlocks, offset);
            offset += sizeof(ulong);

            PersistentMemory.WriteUInt(NextBlockOffset, offset);

            Log.Debug(
                "{RegionsQuantity}|{RegionsSize}|{FreeBlocks}|{NextBlockOffset}",
                RegionsQuantity,
                RegionsSize,
                FreeBlocks,
                NextBlockOffset);
        }

        public void MarkRegionAsFree(byte regionIndex)
        {
            // create a bitmap 
            ulong bitmap = ~(1ul << regionIndex);
            FreeBlocks = FreeBlocks & bitmap;

            UpdateFreeBlocks();
        }

        internal void UpdateNextBlockOffset()
        {
            if (TransactionFile is null) throw new ApplicationException($"Property {nameof(TransactionFile)} cannot be null.");

            TransactionFile.AddRemoveBlockLayout(new RemoveBlockLayout(BlockOffset + Header_FreeBlockBitmapOffset, NextBlockOffset));
            Log.Verbose("Update NextBlockOffset value={value} for block={blockID}", NextBlockOffset, BlockOffset);
        }
    }
}
