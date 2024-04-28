using PM.Core.PMemory.FileFields;
using PM.Core.PMemory.PMemoryTransactionFile;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    public class AddBlockLayout : IBlockLayout
    {
        public CommitByteField CommitByte
        {
            get => _commitByte;
            internal set
            {
                value.Offset = TransactionFileOffset.AddBlockCommitByte;
                _commitByte = value;
            }
        }
        private CommitByteField _commitByte = new(TransactionFileOffset.AddBlockCommitByte);

        public OrderField Order
        {
            get => _order ??= new OrderField(TransactionFileOffset.AddBlockOrder, instance: 1);
            internal set
            {
                if (value != null)
                {
                    value.Offset = TransactionFileOffset.AddBlockOrder;
                    _order = value;
                }
            }
        }
        private OrderField? _order;

        public StartBlockOffsetField StartBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.AddBlockStartBlockOffset);
        public RegionsQttyField RegionsQtty { get; set; } = new RegionsQttyField(TransactionFileOffset.AddBlockRegionsQtty);
        public RegionsSizeField RegionSize { get; set; } = new RegionsSizeField(TransactionFileOffset.AddBlockRegionSize);

        public static AddBlockLayout LoadFromTransactionFile(PmCSharpDefinedTypes transactionFilePm)
        {
            return new AddBlockLayout
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.AddBlockCommitByte, (CommitState)transactionFilePm.ReadByte(TransactionFileOffset.AddBlockCommitByte)),
                Order = new OrderField(offset: TransactionFileOffset.AddBlockOrder, transactionFilePm.ReadUShort(TransactionFileOffset.AddBlockOrder)),
                StartBlockOffset = new StartBlockOffsetField(offset: TransactionFileOffset.AddBlockStartBlockOffset) { Value = transactionFilePm.ReadUInt(TransactionFileOffset.AddBlockStartBlockOffset) },
                RegionsQtty = new RegionsQttyField(TransactionFileOffset.AddBlockRegionsQtty) { Value = transactionFilePm.ReadByte(TransactionFileOffset.AddBlockRegionsQtty) },
                RegionSize = new RegionsSizeField(TransactionFileOffset.AddBlockRegionSize) { Value  = transactionFilePm.ReadUInt(TransactionFileOffset.AddBlockRegionSize) }
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PersistentAllocatorLayout persistentAllocatorLayout)
        {
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout((int)RegionSize.Value, RegionsQtty.Value)
            {
                BlockOffset = StartBlockOffset.Value
            });

            CommitByte.State = CommitState.CommitedAndWriteOnOriginalFileFinished;
            transactionFile.WriteByte(CommitByte.Value, offset: TransactionFileOffset.AddBlockCommitByte);
        }
    }
}
