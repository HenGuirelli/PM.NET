using PM.Core.PMemory.FileFields;
using PM.Core.PMemory.PMemoryTransactionFile;

namespace PM.Core.PMemory.MemoryLayoutTransactions
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

        public OrderField Order
        {
            get => _order ??= new OrderField(TransactionFileOffset.RemoveBlockOrder, instance: 1);
            internal set
            {
                if (value != null)
                {
                    value.Offset = TransactionFileOffset.RemoveBlockOrder;
                    _order = value;
                }
            }
        }
        private OrderField? _order;

        public StartBlockOffsetField StartBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.RemoveBlockStartBlockOffset);


        public RemoveBlockLayout(UInt32 startBlockOffsetField)
        {
            StartBlockOffset = new StartBlockOffsetField(TransactionFileOffset.RemoveBlockStartBlockOffset) { Value = startBlockOffsetField };
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
            return new RemoveBlockLayout(startBlockOffsetField: pmTransactionFile.ReadUInt(TransactionFileOffset.RemoveBlockStartBlockOffset))
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.RemoveBlockCommitByte, (CommitState)pmTransactionFile.ReadByte(TransactionFileOffset.RemoveBlockCommitByte)),
                Order = new OrderField(offset: TransactionFileOffset.RemoveBlockOrder, pmTransactionFile.ReadUShort(TransactionFileOffset.RemoveBlockOrder))
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PersistentAllocatorLayout originalFile)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            pmCSharpDefinedTypes.WriteUShort(Order.Value, offset: TransactionFileOffset.RemoveBlockOrder);
            pmCSharpDefinedTypes.WriteUInt(StartBlockOffset.Value, offset: TransactionFileOffset.RemoveBlockStartBlockOffset);

            // Commit byte need always be the last
            CommitByte.Commit();
            pmCSharpDefinedTypes.WriteByte(CommitByte.Value, offset: TransactionFileOffset.RemoveBlockCommitByte);
        }
    }
}
