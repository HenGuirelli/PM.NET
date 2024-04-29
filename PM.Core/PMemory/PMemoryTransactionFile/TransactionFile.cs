using PM.Core.PMemory.FileFields;
using PM.Core.PMemory.MemoryLayoutTransactions;

namespace PM.Core.PMemory.PMemoryTransactionFile
{
    internal class TransactionFile
    {
        public const ushort TransactionFileVersion = 1;

        private readonly PmCSharpDefinedTypes _pmTransactionFile;
        private readonly PersistentAllocatorLayout _pmOriginalFile;

        private readonly SortedSet<WrapperBlockLayouts> _blocksLayoutsOrdered = new();

        private volatile bool _isPendingTransactionRunning = false;
        private volatile int _transactionFileWriteOperationRunning = 0;

        public TransactionFile(PmCSharpDefinedTypes pmTransactionFile, PersistentAllocatorLayout pmOriginalFile)
        {
            _pmTransactionFile = pmTransactionFile;
            _pmOriginalFile = pmOriginalFile;

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
                _blocksLayoutsOrdered.Add(new WrapperBlockLayouts(addblock!));
            }
            if (RemoveBlockLayout.TryLoadFromTransactionFile(_pmTransactionFile, out var removeblock))
            {
                _blocksLayoutsOrdered.Add(new WrapperBlockLayouts(removeblock!));
            }
            if (UpdateContentBlockLayout.TryLoadFromTransactionFile(_pmTransactionFile, out var updateContentblock))
            {
                _blocksLayoutsOrdered.Add(new WrapperBlockLayouts(updateContentblock!));
            }
        }

        private void ApplyPendingTransaction()
        {
            try
            {
                _isPendingTransactionRunning = true;
                SpinWait.SpinUntil(() => _transactionFileWriteOperationRunning == 0);

                foreach (var block in _blocksLayoutsOrdered)
                {
                    block.Object.ApplyInOriginalFile(_pmTransactionFile, _pmOriginalFile);
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
                addBlockLayout.WriteTo(_pmTransactionFile);
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
                removeBlockLayout.WriteTo(_pmTransactionFile);
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
                if (updateContentBlockLayout.TotalLength > _pmTransactionFile.FileBasedStream.Length)
                {
                    _pmTransactionFile.IncreaseSize(minSize: updateContentBlockLayout.TotalLength);
                }

                updateContentBlockLayout.WriteTo(_pmTransactionFile);
            }
            finally
            {
                _transactionFileWriteOperationRunning--;
            }
        }
    }
}
