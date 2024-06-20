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

        public StartBlockOffsetField BeforeBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.BeforeBlockID);
        public StartBlockOffsetField NextBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.NextBlockID);

        /// <summary>
        /// Remove a block layout from Pmemory
        /// </summary>
        /// <param name="beforeBlockOffset">the block offset that precedes the block to be removed</param>
        /// <param name="nextBlockOffset">the block offset that comes after the block to be removed. Zero if the removed block is the last</param>
        public RemoveBlockLayout(UInt32 beforeBlockOffset, UInt32 nextBlockOffset)
        {
            BeforeBlockOffset = new StartBlockOffsetField(TransactionFileOffset.BeforeBlockID) { Value = beforeBlockOffset };
            NextBlockOffset = new StartBlockOffsetField(TransactionFileOffset.NextBlockID) { Value = nextBlockOffset };
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
            var blockType = transactionFilePm.ReadByte(offset: TransactionFileOffset.HeaderBlockType);
            if (blockType == (byte)BlockLayoutType.RemoveBlock)
            {
                var commitByteState = (CommitState)transactionFilePm.ReadByte(TransactionFileOffset.RemoveBlockCommitByte);
                return commitByteState == CommitState.Commited;
            }
            return false;
        }

        public static RemoveBlockLayout LoadFromTransactionFile(PmCSharpDefinedTypes pmTransactionFile)
        {
            return new RemoveBlockLayout(
                beforeBlockOffset: pmTransactionFile.ReadUInt(TransactionFileOffset.BeforeBlockID),
                nextBlockOffset: pmTransactionFile.ReadUInt(TransactionFileOffset.NextBlockID))
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.RemoveBlockCommitByte, (CommitState)pmTransactionFile.ReadByte(TransactionFileOffset.RemoveBlockCommitByte)),
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PAllocator pAllocator)
        {
            var block = pAllocator.GetBlock(BeforeBlockOffset.Value);

            block.NextBlockOffset = NextBlockOffset.Value;
            block.UpdateNextBlockOffset();

            var newNextBlock = pAllocator.GetBlock(NextBlockOffset.Value);
            block.NextBlock = newNextBlock;

            CommitByte.State = CommitState.CommitedAndWriteOnOriginalFileFinished;
            transactionFile.WriteByte(CommitByte.Value, offset: TransactionFileOffset.RemoveBlockCommitByte);
        }

        public void WriteTo(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            pmCSharpDefinedTypes.WriteUInt(BeforeBlockOffset.Value, offset: TransactionFileOffset.BeforeBlockID);
            pmCSharpDefinedTypes.WriteUInt(NextBlockOffset.Value, offset: TransactionFileOffset.NextBlockID);

            // Commit byte need always be the last
            CommitByte.Commit();
            pmCSharpDefinedTypes.WriteByte(CommitByte.Value, offset: TransactionFileOffset.RemoveBlockCommitByte);
        }
    }
}
