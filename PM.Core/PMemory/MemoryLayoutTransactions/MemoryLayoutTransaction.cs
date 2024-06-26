﻿using PM.Core.PMemory.FileFields;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    public class MemoryLayoutTransaction
    {
        private readonly PmCSharpDefinedTypes _pmCSharpDefinedTypes;
        private readonly PersistentAllocatorLayout _pmOriginalFile;
        private readonly SortedSet<WrapperBlockLayouts> _blocksLayoutOrdened = new();

        // Queue represents a set of "free" offsets.
        // Example: A addblocklayout on file with commit byte is 2 (already written into original file)
        // so, that space can be recycled.
        //
        // Same for corrupted block (commit byte equals 0).
        private readonly Queue<long> _queueAddBlocksFreeOffsets = new();

        class ConstDefinitions
        {
            public const int CommitByte = 0;
            public const int AddBlockOffset = 1;
            public const int AddBlocksQttyOffset = 3;
            public const int RemoveBlockOffset = 5;
            public const int RemoveBlocksQttyOffset = 7;
            public const int UpdateContentOffset = 9;
            public const int UpdateContentQttyOffset = 11;

            public const int InitialAddBlock = 13;
            public const int AddBlockSize = 1000;

            public const int InitialRemoveBlock = InitialAddBlock + AddBlockSize;
            public const int RemoveBlockSize = 500;

            public const int InitialUpdateContentBlock = InitialRemoveBlock + RemoveBlockSize;
            public const int UpdateContentSize = 1_000_000;

            public const int InitialFileSizeInBytes = InitialUpdateContentBlock + UpdateContentSize;
        }

        private uint _addBlockOffset;
        private ushort _addBlockQtty;
        private uint _initialAddBlockOffset;

        private uint _removeBlockOffset;
        private ushort _removeBlockQtty;
        private uint _initialRemoveBlockOffset;

        private uint _updateContentBlockOffset;
        private ushort _updateContentQtty;
        private uint _initialUpdateContentBlockOffset;

        public MemoryLayoutTransaction(PmCSharpDefinedTypes pmTransactionFile, PersistentAllocatorLayout pmOriginalFile = null)
        {
            _pmCSharpDefinedTypes = pmTransactionFile;
            _pmOriginalFile = pmOriginalFile;

            if (_pmCSharpDefinedTypes.FileBasedStream.Length < ConstDefinitions.InitialFileSizeInBytes)
            {
                _pmCSharpDefinedTypes.Resize(ConstDefinitions.InitialFileSizeInBytes);
            }
            if (_pmCSharpDefinedTypes.ReadByte() != 1)
            {
                CreateLayout();
            }

            LoadPointers();

            LoadData();

            CommitAllPendingTransactionsAndDefragTransactionFile();
        }

        private void CommitAllPendingTransactionsAndDefragTransactionFile()
        {
            // Lock all methods

            // Commit all pending transactions

            // Defrag
        }

        private void LoadData()
        {
            _addBlockQtty = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.AddBlocksQttyOffset);
            for (int i = 0; i < _addBlockQtty; i++)
            {
                var offset = _addBlockOffset + (ConstDefinitions.AddBlockSize * i);
                var addBlockData = LoadAddBlock(offset);
                if (addBlockData.CommitByte.State == CommitState.CommitedAndWriteOnOriginalFileFinished ||
                    addBlockData.CommitByte.State == CommitState.NotCommited)
                {
                    _queueAddBlocksFreeOffsets.Enqueue(offset);
                }
                _blocksLayoutOrdened.Add(new WrapperBlockLayouts(addBlockData));
            }

            _removeBlockQtty = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.RemoveBlocksQttyOffset);
            for (int i = 0; i < _removeBlockQtty; i++)
            {
                _blocksLayoutOrdened.Add(new WrapperBlockLayouts(LoadRemoveBlock(_removeBlockOffset + (ConstDefinitions.RemoveBlockSize * i))));
            }

            _updateContentQtty = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.UpdateContentQttyOffset);
            uint lastUpdateContetSize = 0;
            for (int i = 0; i < _updateContentQtty; i++)
            {
                var updateContent = LoadUpdateContent(_updateContentBlockOffset + (lastUpdateContetSize * i));
                lastUpdateContetSize = updateContent.UpdateContentLayoutSize;
                _blocksLayoutOrdened.Add(new WrapperBlockLayouts(updateContent));
            }
        }

        private UpdateContentBlockLayout LoadUpdateContent(long offset)
        {
            return null;
            //var contentSize = _pmCSharpDefinedTypes.ReadUShort(offset + 5);
            //return new UpdateContentBlockLayout
            //{
            //    CommitByte = new CommitByteField(UpdateContentBlockLayout.Offset.CommitByte, (CommitState)_pmCSharpDefinedTypes.ReadByte(offset)),
            //    Order = new OrderField(UpdateContentBlockLayout.Offset.Order, instance: 1, setValue: _pmCSharpDefinedTypes.ReadUShort(offset + 1)),
            //    BlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset + 1),
            //    ContentSize = contentSize,
            //    Content = _pmCSharpDefinedTypes.ReadBytes(contentSize, offset + 9)
            //};
        }

        private RemoveBlockLayout LoadRemoveBlock(long offset)
        {
            return null;
            //return new RemoveBlockLayout
            //{
            //    CommitByte = new CommitByteField(RemoveBlockLayout.Offset.CommitByte, (CommitState)_pmCSharpDefinedTypes.ReadByte(offset)),
            //    Order = new OrderField(RemoveBlockLayout.Offset.Order, instance: 1, setValue: _pmCSharpDefinedTypes.ReadUShort(offset + 1)),
            //    StartBlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset + 3),
            //};
        }

        private AddBlockLayout LoadAddBlock(long offset)
        {
            return null;
            //return new AddBlockLayout
            //{
            //    CommitByte = new CommitByteField(AddBlockLayout.Offset.CommitByte, (CommitState)_pmCSharpDefinedTypes.ReadByte(offset)),
            //    Order = new OrderField(AddBlockLayout.Offset.Order, instance: 1, setValue: _pmCSharpDefinedTypes.ReadUShort(offset + 1)),
            //    StartBlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset + 3),
            //    RegionsQtty = _pmCSharpDefinedTypes.ReadByte(offset + 7),
            //    RegionSize = _pmCSharpDefinedTypes.ReadUShort(offset + 8)
            //};
        }

        private void LoadPointers()
        {
            _addBlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.AddBlockOffset);
            _initialAddBlockOffset = _addBlockOffset;

            _removeBlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.RemoveBlockOffset);
            _initialRemoveBlockOffset = _removeBlockOffset;

            _updateContentBlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.UpdateContentOffset);
            _initialUpdateContentBlockOffset = _updateContentBlockOffset;
        }

        private void CreateLayout()
        {
            _pmCSharpDefinedTypes.WriteUShort(ConstDefinitions.InitialAddBlock, offset: ConstDefinitions.AddBlockOffset);
            _pmCSharpDefinedTypes.WriteUShort(0, offset: ConstDefinitions.AddBlocksQttyOffset);
            _pmCSharpDefinedTypes.WriteUShort(ConstDefinitions.InitialRemoveBlock, offset: ConstDefinitions.RemoveBlockOffset);
            _pmCSharpDefinedTypes.WriteUShort(0, offset: ConstDefinitions.RemoveBlocksQttyOffset);
            _pmCSharpDefinedTypes.WriteUShort(ConstDefinitions.InitialUpdateContentBlock, offset: ConstDefinitions.UpdateContentOffset);
            _pmCSharpDefinedTypes.WriteUShort(0, offset: ConstDefinitions.UpdateContentQttyOffset);

            _pmCSharpDefinedTypes.WriteByte(1, offset: ConstDefinitions.CommitByte);
        }

        public void AddBlock(AddBlockLayout addBlock)
        {
            //if (_addBlockOffset + AddBlockLayout.Size >= _initialRemoveBlockOffset)
            //{
            //    throw new ApplicationException("Not enough space for a new block (add) on trasaction file");
            //}

            //// Try get a free offset to recycle.
            //if (!_queueAddBlocksFreeOffsets.TryDequeue(out long internalOffset))
            //{
            //    // If not have any free blocks to recycle then add next block into a file.
            //    internalOffset = _addBlockOffset;
            //}

            //addBlock.TransactionOffset = internalOffset;

            //addBlock.CommitByte.UnCommit();
            //_pmCSharpDefinedTypes.WriteByte(addBlock.CommitByte.Value, offset: internalOffset);

            //internalOffset += 1;
            //_pmCSharpDefinedTypes.WriteUInt(addBlock.Order.Value, offset: internalOffset);
            //internalOffset += 2;
            //_pmCSharpDefinedTypes.WriteUInt(addBlock.StartBlockOffset, offset: internalOffset);
            //internalOffset += 4;
            //_pmCSharpDefinedTypes.WriteByte(addBlock.RegionsQtty, offset: internalOffset);
            //internalOffset += 1;
            //_pmCSharpDefinedTypes.WriteUInt(addBlock.RegionSize, offset: internalOffset);

            //addBlock.CommitByte.Commit();
            //_pmCSharpDefinedTypes.WriteByte(addBlock.CommitByte.Value, offset: _addBlockOffset);

            //_addBlockQtty++;
            //_pmCSharpDefinedTypes.WriteUShort(_addBlockQtty, ConstDefinitions.AddBlocksQttyOffset);

            //_addBlockOffset += AddBlockLayout.Size;

            //_blocksLayoutOrdened.Add(new WrapperBlockLayouts(addBlock));
        }

        public void CommitBlockLayouts(int? qtty = null)
        {
            if (qtty == null)
            {
                foreach (var item in _blocksLayoutOrdened)
                {
                    CommitBlockLayout(item);
                }
            }
            else
            {
                int count = 0;
                foreach (var item in _blocksLayoutOrdened)
                {
                    if (count >= qtty) return;

                    CommitBlockLayout(item);
                    count++;
                }
            }
        }

        private void CommitBlockLayout(WrapperBlockLayouts item)
        {
            //if (item.BlockLayoutType == BlockLayoutType.AddBlock)
            //{
            //    var addBlockLayout = (AddBlockLayout)item.Object;
            //    _pmOriginalFile.AddBlock(new PersistentBlockLayout((int)addBlockLayout.RegionSize, addBlockLayout.RegionsQtty)
            //    {
            //        BlockOffset = addBlockLayout.StartBlockOffset
            //    });

            //    addBlockLayout.CommitByte.State = CommitState.CommitedAndWriteOnOriginalFileFinished;
            //    _pmCSharpDefinedTypes.WriteByte(addBlockLayout.CommitByte.Value, offset: addBlockLayout.TransactionOffset);
            //}
        }

        public void RemoveBlock(RemoveBlockLayout removeBlock)
        {
            //if (_removeBlockOffset + RemoveBlockLayout.Size >= _initialUpdateContentBlockOffset)
            //{
            //    throw new ApplicationException("Not enough space for a new block (remove) on trasaction file");
            //}

            //removeBlock.CommitByte.UnCommit();
            //_pmCSharpDefinedTypes.WriteByte(removeBlock.CommitByte.Value, offset: _removeBlockOffset);

            //_pmCSharpDefinedTypes.WriteUInt(removeBlock.StartBlockOffset, offset: _removeBlockOffset + 1);

            //removeBlock.CommitByte.Commit();
            //_pmCSharpDefinedTypes.WriteByte(removeBlock.CommitByte.Value, offset: _removeBlockOffset);

            //_removeBlockOffset += RemoveBlockLayout.Size;
        }

        public void UpdateContent(UpdateContentBlockLayout updateContent)
        {
            if (_updateContentBlockOffset + updateContent.UpdateContentLayoutSize <= _pmCSharpDefinedTypes.FileBasedStream.Length)
            {
                _pmCSharpDefinedTypes.IncreaseSize();
            }

            updateContent.CommitByte.UnCommit();
            _pmCSharpDefinedTypes.WriteByte(updateContent.CommitByte.Value, offset: _updateContentBlockOffset);

            _pmCSharpDefinedTypes.WriteUInt(updateContent.BlockOffset, offset: _updateContentBlockOffset + 1);

            updateContent.CommitByte.Commit();
            _pmCSharpDefinedTypes.WriteByte(updateContent.CommitByte.Value, offset: _updateContentBlockOffset);

            _updateContentBlockOffset += updateContent.UpdateContentLayoutSize;
        }
    }
}
