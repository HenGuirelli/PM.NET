using PM.Common;
using PM.FileEngine.FileFields;

namespace PM.FileEngine.Transactions
{
    public class RemoveBlockLayout : IBlockLayout
    {
        public CommitByteField CommitByte
        {
            get => _commitByte;
            set
            {
                value.Offset = TransactionFileOffset.RemoveBlockCommitByte;
                _commitByte = value;
            }
        }
        private CommitByteField _commitByte = new(TransactionFileOffset.RemoveBlockCommitByte);

        public StartBlockOffsetField BeforeBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.RemoveBlockBeforeBlockOffset);
        public StartBlockOffsetField RemovedBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.RemoveBlockRemovedBlockOffset);
        public StartBlockOffsetField AfterBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.RemoveBlockAfterBlockOffset);

        /// <summary>
        /// Remove a block layout from Pmemory
        /// </summary>
        /// <param name="beforeBlockOffset">the block offset that precedes the block to be removed</param>
        /// <param name="removedBlockOffset">the block offset to be removed</param>
        /// <param name="afterBlockOffset">the block offset that comes after the block to be removed. Zero if the removed block is the last</param>
        public RemoveBlockLayout(UInt32 beforeBlockOffset, UInt32 removedBlockOffset, UInt32 afterBlockOffset)
        {
            BeforeBlockOffset = new StartBlockOffsetField(TransactionFileOffset.RemoveBlockBeforeBlockOffset) { Value = beforeBlockOffset };
            RemovedBlockOffset = new StartBlockOffsetField(TransactionFileOffset.RemoveBlockRemovedBlockOffset) { Value = removedBlockOffset };
            AfterBlockOffset = new StartBlockOffsetField(TransactionFileOffset.RemoveBlockAfterBlockOffset) { Value = afterBlockOffset };
        }

        public static bool TryLoadFromTransactionFile(PmCSharpDefinedTypes transactionFilePm, out RemoveBlockLayout? result)
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
            var commitByteState = (CommitState)transactionFilePm.ReadByte(TransactionFileOffset.RemoveBlockCommitByte);
            return commitByteState == CommitState.Commited;
        }

        public static RemoveBlockLayout LoadFromTransactionFile(PmCSharpDefinedTypes pmTransactionFile)
        {
            return new RemoveBlockLayout(
                beforeBlockOffset: pmTransactionFile.ReadUInt(TransactionFileOffset.RemoveBlockBeforeBlockOffset),
                removedBlockOffset: pmTransactionFile.ReadUInt(TransactionFileOffset.RemoveBlockRemovedBlockOffset),
                afterBlockOffset: pmTransactionFile.ReadUInt(TransactionFileOffset.RemoveBlockAfterBlockOffset))
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.RemoveBlockCommitByte, (CommitState)pmTransactionFile.ReadByte(TransactionFileOffset.RemoveBlockCommitByte)),
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PAllocator pAllocator)
        {
            var nextBlockLayoutStartAddress_localOffset = 14;

            throw new ApplicationException("Veriricar se é primeiro ou ultimo bloco");

            pAllocator.PersistentMemory.WriteUInt(AfterBlockOffset.Value, BeforeBlockOffset.Value + nextBlockLayoutStartAddress_localOffset);

            CommitByte.State = CommitState.CommitedAndWriteOnOriginalFileFinished;
            transactionFile.WriteByte(CommitByte.Value, offset: TransactionFileOffset.RemoveBlockCommitByte);
        }

        public void WriteTo(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            pmCSharpDefinedTypes.WriteUInt(BeforeBlockOffset.Value, offset: TransactionFileOffset.RemoveBlockBeforeBlockOffset);
            pmCSharpDefinedTypes.WriteUInt(RemovedBlockOffset.Value, offset: TransactionFileOffset.RemoveBlockRemovedBlockOffset);
            pmCSharpDefinedTypes.WriteUInt(AfterBlockOffset.Value, offset: TransactionFileOffset.RemoveBlockAfterBlockOffset);

            // Commit byte need always be the last
            CommitByte.Commit();
            pmCSharpDefinedTypes.WriteByte(CommitByte.Value, offset: TransactionFileOffset.RemoveBlockCommitByte);
        }
    }
}
