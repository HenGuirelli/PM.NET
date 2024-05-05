using PM.Common;
using PM.FileEngine.FileFields;

namespace PM.FileEngine.Transactions
{
    public class TransactionFile
    {
        public const ushort TransactionFileVersion = 1;

        private readonly PmCSharpDefinedTypes _pmTransactionFile;
        private readonly PAllocator _pmAllocator;

        private WrapperBlockLayout? _pendingBlockLayout;

        public bool HasPendingTransactions => _pendingBlockLayout != null;
        public string FilePath => _pmTransactionFile.FilePath;

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
            return _pmTransactionFile.ReadByte(offset: TransactionFileOffset.HeaderCommitByte) == 0;
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
                _pendingBlockLayout = new WrapperBlockLayout(addblock!);
            }
            if (RemoveBlockLayout.TryLoadFromTransactionFile(_pmTransactionFile, out var removeblock))
            {
                _pendingBlockLayout = new WrapperBlockLayout(removeblock!);
            }
            if (UpdateContentBlockLayout.TryLoadFromTransactionFile(_pmTransactionFile, out var updateContentblock))
            {
                _pendingBlockLayout = new WrapperBlockLayout(updateContentblock!);
            }
            if (UpdateFreeBlocksFromBlockLayout.TryLoadFromTransactionFile(_pmTransactionFile, out var updateFreeBlocksFromBlockLayout))
            {
                _pendingBlockLayout = new WrapperBlockLayout(updateFreeBlocksFromBlockLayout!);
            }
        }

        public void ApplyPendingTransaction()
        {
            if (HasPendingTransactions)
            {
                _pendingBlockLayout!.Object.ApplyInOriginalFile(_pmTransactionFile, _pmAllocator);
                _pendingBlockLayout = null;
            }
        }

        public void AddNewBlockLayout(AddBlockLayout addBlockLayout)
        {
            if (HasPendingTransactions)
            {
                ApplyPendingTransaction();
            }

            UpdateHeaderBlockType(BlockLayoutType.AddBlock);
            addBlockLayout.WriteTo(_pmTransactionFile);
            _pendingBlockLayout = new WrapperBlockLayout(addBlockLayout);

            ApplyPendingTransaction();
        }

        public void UpdateFreeBlocksLayout(UpdateFreeBlocksFromBlockLayout updateFreeBlocksFromBlockLayout)
        {
            if (HasPendingTransactions)
            {
                ApplyPendingTransaction();
            }

            UpdateHeaderBlockType(BlockLayoutType.UpdateFreeBlocksFromBlock);
            updateFreeBlocksFromBlockLayout.WriteTo(_pmTransactionFile);
            _pendingBlockLayout = new WrapperBlockLayout(updateFreeBlocksFromBlockLayout);

            ApplyPendingTransaction();
        }

        public void AddRemoveBlockLayout(RemoveBlockLayout removeBlockLayout)
        {
            if (HasPendingTransactions)
            {
                ApplyPendingTransaction();
            }

            UpdateHeaderBlockType(BlockLayoutType.RemoveBlock);
            removeBlockLayout.WriteTo(_pmTransactionFile);
            _pendingBlockLayout = new WrapperBlockLayout(removeBlockLayout);

            ApplyPendingTransaction();
        }

        public void AddUpdateContentBlockLayout(UpdateContentBlockLayout updateContentBlockLayout)
        {
            if (HasPendingTransactions)
            {
                ApplyPendingTransaction();
            }

            if (updateContentBlockLayout.TotalLength > _pmTransactionFile.FileBasedStream.Length)
            {
                _pmTransactionFile.IncreaseSize(minSize: updateContentBlockLayout.TotalLength);
            }

            UpdateHeaderBlockType(BlockLayoutType.UpdateContentBlock);
            updateContentBlockLayout.WriteTo(_pmTransactionFile);
            _pendingBlockLayout = new WrapperBlockLayout(updateContentBlockLayout);

            ApplyPendingTransaction();
        }

        private void UpdateHeaderBlockType(BlockLayoutType blockLayout)
        {
            _pmTransactionFile.WriteByte((byte)blockLayout, offset: TransactionFileOffset.HeaderBlockType);
        }
    }
}
