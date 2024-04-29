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

        public AddBlockLayout(
            UInt32 startBlockOffsetField,
            byte regionsQttyField,
            UInt32 regionsSizeField)
        {
            StartBlockOffset = new StartBlockOffsetField(TransactionFileOffset.AddBlockStartBlockOffset) { Value = startBlockOffsetField };
            RegionsQtty = new RegionsQttyField(TransactionFileOffset.AddBlockRegionsQtty) { Value = regionsQttyField };
            RegionSize = new RegionsSizeField(TransactionFileOffset.AddBlockRegionSize) { Value = regionsSizeField };
        }

        public static bool TryLoadFromTransactionFile(PmCSharpDefinedTypes transactionFilePm, out AddBlockLayout? result)
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
            var commitByteState = (CommitState)transactionFilePm.ReadByte(TransactionFileOffset.AddBlockCommitByte);
            return commitByteState == CommitState.Commited;
        }

        public static AddBlockLayout LoadFromTransactionFile(PmCSharpDefinedTypes transactionFilePm)
        {
            return new AddBlockLayout(
                startBlockOffsetField: transactionFilePm.ReadUInt(TransactionFileOffset.AddBlockStartBlockOffset),
                regionsQttyField: transactionFilePm.ReadByte(TransactionFileOffset.AddBlockRegionsQtty),
                regionsSizeField: transactionFilePm.ReadUInt(TransactionFileOffset.AddBlockRegionSize))
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.AddBlockCommitByte, (CommitState)transactionFilePm.ReadByte(TransactionFileOffset.AddBlockCommitByte)),
                Order = new OrderField(offset: TransactionFileOffset.AddBlockOrder, transactionFilePm.ReadUShort(TransactionFileOffset.AddBlockOrder)),
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

        public void WriteTo(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            pmCSharpDefinedTypes.WriteUShort(Order.Value, offset: TransactionFileOffset.AddBlockOrder);
            pmCSharpDefinedTypes.WriteUInt(StartBlockOffset.Value, offset: TransactionFileOffset.AddBlockStartBlockOffset);
            pmCSharpDefinedTypes.WriteByte(RegionsQtty.Value, offset: TransactionFileOffset.AddBlockRegionsQtty);
            pmCSharpDefinedTypes.WriteUInt(RegionSize.Value, offset: TransactionFileOffset.AddBlockRegionSize);

            // Commit byte need always be the last
            CommitByte.Commit();
            pmCSharpDefinedTypes.WriteByte(CommitByte.Value, offset: TransactionFileOffset.AddBlockCommitByte);
        }
    }
}
