using PM.Core.PMemory.FileFields;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    public class MemoryLayoutTransaction
    {
        private readonly PmCSharpDefinedTypes _pmCSharpDefinedTypes;

        private Queue<AddBlockLayout> _queueAddBlockLayouts = new();
        private Queue<RemoveBlockLayout> _queueRemoveBlockLayouts = new();
        private Queue<UpdateContentLayout> _queueUpdateContentLayouts = new();

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
        private uint _initialAddBlockOffset;

        private uint _removeBlockOffset;
        private uint _initialRemoveBlockOffset;

        private uint _updateContentBlockOffset;
        private uint _initialUpdateContentBlockOffset;

        public MemoryLayoutTransaction(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            _pmCSharpDefinedTypes = pmCSharpDefinedTypes;

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
        }

        private void LoadData()
        {
            var addBlocksQtty = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.AddBlocksQttyOffset);
            for (int i = 0; i < addBlocksQtty; i++)
            {
                _queueAddBlockLayouts.Enqueue(LoadAddBlock(_addBlockOffset + (ConstDefinitions.AddBlockSize * i)));
            }

            var removeBlocksQtty = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.RemoveBlocksQttyOffset);
            for (int i = 0; i < removeBlocksQtty; i++)
            {
                _queueRemoveBlockLayouts.Enqueue(LoadRemoveBlock(_removeBlockOffset + (ConstDefinitions.RemoveBlockSize * i)));
            }

            var updateContentQtty = _pmCSharpDefinedTypes.ReadUShort(offset: ConstDefinitions.UpdateContentQttyOffset);
            uint lastUpdateContetSize = 0;
            for (int i = 0; i < updateContentQtty; i++)
            {
                var updateContent = LoadUpdateContent(_updateContentBlockOffset + (lastUpdateContetSize * i));
                lastUpdateContetSize = updateContent.UpdateContentLayoutSize;
                _queueUpdateContentLayouts.Enqueue(updateContent);
            }
        }

        private UpdateContentLayout LoadUpdateContent(long offset)
        {
            var contentSize = _pmCSharpDefinedTypes.ReadUShort(offset + 5);
            return new UpdateContentLayout
            {
                CommitByte = new CommitByteField(UpdateContentLayout.Offset.CommitByte, (CommitState)_pmCSharpDefinedTypes.ReadByte(offset)),
                Order = new OrderField(UpdateContentLayout.Offset.Order) { Value = _pmCSharpDefinedTypes.ReadUShort(offset + 1) },
                BlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset + 1),
                ContentSize = contentSize,
                Content = _pmCSharpDefinedTypes.ReadBytes(contentSize, offset + 9)
            };
        }

        private RemoveBlockLayout LoadRemoveBlock(long offset)
        {
            return new RemoveBlockLayout
            {
                CommitByte = new CommitByteField(RemoveBlockLayout.Offset.CommitByte, (CommitState)_pmCSharpDefinedTypes.ReadByte(offset)),
                Order = new OrderField(UpdateContentLayout.Offset.Order) { Value = _pmCSharpDefinedTypes.ReadUShort(offset + 1) },
                BlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset + 3),
            };
        }

        private AddBlockLayout LoadAddBlock(long offset)
        {
            return new AddBlockLayout
            {
                CommitByte = new CommitByteField(AddBlockLayout.Offset.CommitByte, (CommitState)_pmCSharpDefinedTypes.ReadByte(offset)),
                Order = new OrderField(UpdateContentLayout.Offset.Order) { Value = _pmCSharpDefinedTypes.ReadUShort(offset + 1) },
                BlockOffset = _pmCSharpDefinedTypes.ReadUShort(offset + 3),
                RegionsQtty = _pmCSharpDefinedTypes.ReadByte(offset + 7),
                RegionSize = _pmCSharpDefinedTypes.ReadUShort(offset + 8)
            };
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
            if (_addBlockOffset + AddBlockLayout.Size >= _initialRemoveBlockOffset)
            {
                throw new ApplicationException("Not enough space for a new block (add) on trasaction file");
            }

            addBlock.CommitByte.UnCommit();
            _pmCSharpDefinedTypes.WriteByte(addBlock.CommitByte.Value, offset: _addBlockOffset);

            _pmCSharpDefinedTypes.WriteUInt(addBlock.BlockOffset, offset: _addBlockOffset + 1);
            _pmCSharpDefinedTypes.WriteByte(addBlock.RegionsQtty, offset: _addBlockOffset + 5);
            _pmCSharpDefinedTypes.WriteUInt(addBlock.RegionSize, offset: _addBlockOffset + 6);

            addBlock.CommitByte.Commit();
            _pmCSharpDefinedTypes.WriteByte(addBlock.CommitByte.Value, offset: _addBlockOffset);

            _addBlockOffset += AddBlockLayout.Size;
        }

        public void RemoveBlock(RemoveBlockLayout removeBlock)
        {
            if (_removeBlockOffset + RemoveBlockLayout.Size >= _initialUpdateContentBlockOffset)
            {
                throw new ApplicationException("Not enough space for a new block (remove) on trasaction file");
            }

            removeBlock.CommitByte.UnCommit();
            _pmCSharpDefinedTypes.WriteByte(removeBlock.CommitByte.Value, offset: _removeBlockOffset);

            _pmCSharpDefinedTypes.WriteUInt(removeBlock.BlockOffset, offset: _removeBlockOffset + 1);

            removeBlock.CommitByte.Commit();
            _pmCSharpDefinedTypes.WriteByte(removeBlock.CommitByte.Value, offset: _removeBlockOffset);

            _removeBlockOffset += RemoveBlockLayout.Size;
        }

        public void UpdateContent(UpdateContentLayout updateContent)
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
