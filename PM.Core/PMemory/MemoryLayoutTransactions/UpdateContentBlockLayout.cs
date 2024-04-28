using PM.Core.PMemory.FileFields;
using PM.Core.PMemory.PMemoryTransactionFile;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    public class UpdateContentBlockLayout : IBlockLayout
    {
        public CommitByteField CommitByte
        {
            get => _commitByte;
            set
            {
                value.Offset = TransactionFileOffset.UpdateContentBlockCommitByte;
                _commitByte = value;
            }
        }
        private CommitByteField _commitByte = new(TransactionFileOffset.UpdateContentBlockCommitByte);

        public OrderField Order
        {
            get => _order ??= new OrderField(TransactionFileOffset.UpdateContentBlockOrder, instance: 1);
            internal set
            {
                if (value != null)
                {
                    value.Offset = TransactionFileOffset.UpdateContentBlockOrder;
                    _order = value;
                }
            }
        }
        private OrderField? _order;

        public UInt32 BlockOffset { get; set; }
        public UInt32 ContentSize { get; set; }
        public byte[]? Content { get; set; }

        internal uint UpdateContentLayoutSize => 9 + ContentSize;

        public static RemoveBlockLayout LoadFromTransactionFile(PmCSharpDefinedTypes pmTransactionFile)
        {
            return new RemoveBlockLayout
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.UpdateContentBlockCommitByte, (CommitState)pmTransactionFile.ReadByte(TransactionFileOffset.UpdateContentBlockCommitByte)),
                Order = new OrderField(offset: TransactionFileOffset.UpdateContentBlockOrder, pmTransactionFile.ReadUShort(TransactionFileOffset.UpdateContentBlockOrder)),
                StartBlockOffset = new StartBlockOffsetField(offset: TransactionFileOffset.UpdateContentBlockStartBlockOffset) { Value = pmTransactionFile.ReadUInt(TransactionFileOffset.UpdateContentBlockStartBlockOffset) },
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PersistentAllocatorLayout originalFile)
        {
            throw new NotImplementedException();
        }
    }
}
