using PM.Core.PMemory.FileFields;
using PM.Core.PMemory.MemoryLayoutTransactions;

namespace PM.Core.PMemory.PMemoryTransactionFile
{
    internal class TransactionFile
    {
        public const ushort TransactionFileVersion = 1;

        private readonly PmCSharpDefinedTypes _pmTransactionFile;
        private readonly PAllocator _pmAllocator;

        private readonly Dictionary<BlockLayoutType, WrapperBlockLayouts> _blocksLayoutsByType = new();
        private readonly SortedSet<WrapperBlockLayouts> _blocksLayoutsOrdered = new();

        private volatile bool _isPendingTransactionRunning = false;
        private volatile int _transactionFileWriteOperationRunning = 0;

        public TransactionFile(PmCSharpDefinedTypes pmTransactionFile, PAllocator pAllocator)
        {
            _pmTransactionFile = pmTransactionFile;
            _pmAllocator = pAllocator;

            if (pmTransactionFile.FileBasedStream.Length < 100)
            {
                pmTransactionFile.IncreaseSize(minSize: 100);
            }

            if (IsFirstExecution())
            {
                CreateHeaderFile();
            }

            if (!IsValidVersion(out ushort actualVersion))
            {
                throw new TransacionFileVersionException(TransactionFileVersion, actualVersion);
            }

            LoadTransactionFileIntoMemory();

            ApplyPendingTransaction();
        }

        private bool IsValidVersion(out ushort actualVersion)
        {
            actualVersion = _pmTransactionFile.ReadUShort(offset: TransactionFileOffset.HeaderVersion);
            return actualVersion == TransactionFileVersion;
        }

        private bool IsFirstExecution()
        {
            return _pmTransactionFile.ReadByte() == 0;
        }

        private void CreateHeaderFile()
        {
            _pmTransactionFile.WriteUShort(TransactionFileVersion, offset: TransactionFileOffset.HeaderVersion);
            _pmTransactionFile.WriteByte((byte)CommitState.Commited, offset: TransactionFileOffset.HeaderCommitByte);
        }

        private void LoadTransactionFileIntoMemory()
        {
            if (AddBlockLayout.TryLoadFromTransactionFile(_pmTransactionFile, out var addblock))
            {
                var addWrapper = new WrapperBlockLayouts(addblock!);
                _blocksLayoutsByType[BlockLayoutType.AddBlock] = addWrapper;
                _blocksLayoutsOrdered.Add(addWrapper);
            }
            if (RemoveBlockLayout.TryLoadFromTransactionFile(_pmTransactionFile, out var removeblock))
            {
                var removeWrapper = new WrapperBlockLayouts(removeblock!);
                _blocksLayoutsByType[BlockLayoutType.RemoveBlock] = removeWrapper;
                _blocksLayoutsOrdered.Add(removeWrapper);
            }
            if (UpdateContentBlockLayout.TryLoadFromTransactionFile(_pmTransactionFile, out var updateContentblock))
            {
                var updateContentWrapper = new WrapperBlockLayouts(updateContentblock!);
                _blocksLayoutsByType[BlockLayoutType.UpdateContentBlock] = updateContentWrapper;
                _blocksLayoutsOrdered.Add(updateContentWrapper);
            }
        }

        public void ApplyPendingTransaction()
        {
            try
            {
                _isPendingTransactionRunning = true;
                SpinWait.SpinUntil(() => _transactionFileWriteOperationRunning == 0);

                foreach (var block in _blocksLayoutsOrdered)
                {
                    block.Object.ApplyInOriginalFile(_pmTransactionFile, _pmAllocator);
                    _blocksLayoutsByType.Remove(block.BlockLayoutType);
                }
            }
            finally
            {
                _isPendingTransactionRunning = false;
            }
        }

        public void AddNewBlockLayout(AddBlockLayout addBlockLayout)
        {
            try
            {
                SpinWait.SpinUntil(() => !_isPendingTransactionRunning);
                _transactionFileWriteOperationRunning++;

                if (_blocksLayoutsByType.ContainsKey(BlockLayoutType.AddBlock))
                {
                    throw new ApplicationException("Block still not commited");
                }

                addBlockLayout.WriteTo(_pmTransactionFile);
                var addWrapper = new WrapperBlockLayouts(addBlockLayout);
                _blocksLayoutsOrdered.Add(addWrapper);
                _blocksLayoutsByType[BlockLayoutType.AddBlock] = addWrapper;
            }
            finally
            {
                _transactionFileWriteOperationRunning--;
            }
        }

        public void AddRemoveBlockLayout(RemoveBlockLayout removeBlockLayout)
        {
            try
            {
                SpinWait.SpinUntil(() => !_isPendingTransactionRunning);
                _transactionFileWriteOperationRunning++;


                if (_blocksLayoutsByType.ContainsKey(BlockLayoutType.RemoveBlock))
                {
                    throw new ApplicationException("Block still not commited");
                }

                removeBlockLayout.WriteTo(_pmTransactionFile);
                var removeWrapper = new WrapperBlockLayouts(removeBlockLayout);
                _blocksLayoutsOrdered.Add(removeWrapper);
                _blocksLayoutsByType[BlockLayoutType.RemoveBlock] = removeWrapper;
            }
            finally
            {
                _transactionFileWriteOperationRunning--;
            }
        }

        public void AddUpdateContentBlockLayout(UpdateContentBlockLayout updateContentBlockLayout)
        {
            try
            {
                SpinWait.SpinUntil(() => !_isPendingTransactionRunning);
                _transactionFileWriteOperationRunning++;


                if (_blocksLayoutsByType.ContainsKey(BlockLayoutType.UpdateContentBlock))
                {
                    throw new ApplicationException("Block still not commited");
                }

                if (updateContentBlockLayout.TotalLength > _pmTransactionFile.FileBasedStream.Length)
                {
                    _pmTransactionFile.IncreaseSize(minSize: updateContentBlockLayout.TotalLength);
                }

                updateContentBlockLayout.WriteTo(_pmTransactionFile);
                var updateContentWrapper = new WrapperBlockLayouts(updateContentBlockLayout);
                _blocksLayoutsOrdered.Add(updateContentWrapper);
                _blocksLayoutsByType[BlockLayoutType.UpdateContentBlock] = updateContentWrapper;
            }
            finally
            {
                _transactionFileWriteOperationRunning--;
            }
        }
    }
}
