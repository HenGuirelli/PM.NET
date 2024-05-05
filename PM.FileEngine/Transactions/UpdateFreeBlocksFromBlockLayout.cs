using PM.Common;
using PM.FileEngine.FileFields;

namespace PM.FileEngine.Transactions
{
    public class UpdateFreeBlocksFromBlockLayout : IBlockLayout
    {
        public CommitByteField CommitByte
        {
            get => _commitByte;
            set
            {
                value.Offset = TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutCommitByte;
                _commitByte = value;
            }
        }
        private CommitByteField _commitByte = new(TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutCommitByte);

        /// <summary>
        /// FreeBlocks absolute offset
        /// </summary>
        public StartBlockOffsetField BlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutStartBlockOffset);
        public NewFreeBlocksField NewFreeBlocks { get; set; } = new NewFreeBlocksField(TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutFreeBlocksValue);

        public UpdateFreeBlocksFromBlockLayout(uint startBlockOffset, ulong freeBlocks)
        {
            BlockOffset = new StartBlockOffsetField(offset: TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutStartBlockOffset) { Value = startBlockOffset };
            NewFreeBlocks = new NewFreeBlocksField(offset: TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutFreeBlocksValue) { Value = freeBlocks };
        }

        public static bool TryLoadFromTransactionFile(PmCSharpDefinedTypes transactionFilePm, out UpdateFreeBlocksFromBlockLayout? result)
        {
            if (IsPendingTransaction(transactionFilePm))
            {
                result = LoadFromTransactionFile(transactionFilePm);
                return true;
            }
            result = null;
            return false;
        }

        private static bool IsPendingTransaction(PmCSharpDefinedTypes transactionFilePm)
        {
            var blockType = transactionFilePm.ReadByte(offset: TransactionFileOffset.HeaderBlockType);
            if (blockType == (byte)BlockLayoutType.UpdateFreeBlocksFromBlock)
            {
                var commitByteState = (CommitState)transactionFilePm.ReadByte(TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutCommitByte);
                return commitByteState == CommitState.Commited;
            }
            return false;
        }

        public static UpdateFreeBlocksFromBlockLayout LoadFromTransactionFile(PmCSharpDefinedTypes pmTransactionFile)
        {
            return new UpdateFreeBlocksFromBlockLayout(
                pmTransactionFile.ReadUInt(TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutStartBlockOffset),
                pmTransactionFile.ReadULong(TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutFreeBlocksValue))
            {
                CommitByte = new CommitByteField(
                    offset: TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutCommitByte,
                    commitState: (CommitState)pmTransactionFile.ReadByte(TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutCommitByte)),
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PAllocator pAllocator)
        {
            pAllocator.PersistentMemory.WriteULong(NewFreeBlocks.Value, offset: BlockOffset.Value);

            CommitByte.State = CommitState.CommitedAndWriteOnOriginalFileFinished;
            transactionFile.WriteByte(CommitByte.Value, offset: TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutCommitByte);
        }

        public void WriteTo(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            pmCSharpDefinedTypes.WriteUInt(BlockOffset.Value, offset: TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutStartBlockOffset);
            pmCSharpDefinedTypes.WriteULong(NewFreeBlocks.Value, offset: TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutFreeBlocksValue);

            // Commit byte need always be the last
            CommitByte.Commit();
            pmCSharpDefinedTypes.WriteByte(CommitByte.Value, offset: TransactionFileOffset.UpdateFreeBlocksFromBlockLayoutCommitByte);
        }
    }
}
