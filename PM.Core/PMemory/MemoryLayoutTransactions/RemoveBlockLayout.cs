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

        public static RemoveBlockLayout LoadFromTransactionFile(PmCSharpDefinedTypes pmTransactionFile)
        {
            return new RemoveBlockLayout
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.RemoveBlockCommitByte, (CommitState)pmTransactionFile.ReadByte(TransactionFileOffset.RemoveBlockCommitByte)),
                Order = new OrderField(offset: TransactionFileOffset.RemoveBlockOrder, pmTransactionFile.ReadUShort(TransactionFileOffset.RemoveBlockOrder)),
                StartBlockOffset = new StartBlockOffsetField(offset: TransactionFileOffset.RemoveBlockStartBlockOffset) { Value = pmTransactionFile.ReadUInt(TransactionFileOffset.RemoveBlockStartBlockOffset) },
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PersistentAllocatorLayout originalFile)
        {
            throw new NotImplementedException();
        }
    }
}
