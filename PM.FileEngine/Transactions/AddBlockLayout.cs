using PM.Core.PMemory;
using PM.FileEngine.FileFields;

namespace PM.FileEngine.Transactions
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

        public StartBlockOffsetField StartBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.AddBlockStartBlockOffset);
        public RegionsQttyField RegionsQtty { get; set; } = new RegionsQttyField(TransactionFileOffset.AddBlockRegionsQtty);
        public RegionsSizeField RegionSize { get; set; } = new RegionsSizeField(TransactionFileOffset.AddBlockRegionSize);

        public AddBlockLayout(
            uint startBlockOffset,
            byte regionsQtty,
            uint regionsSize)
        {
            StartBlockOffset = new StartBlockOffsetField(TransactionFileOffset.AddBlockStartBlockOffset) { Value = startBlockOffset };
            RegionsQtty = new RegionsQttyField(TransactionFileOffset.AddBlockRegionsQtty) { Value = regionsQtty };
            RegionSize = new RegionsSizeField(TransactionFileOffset.AddBlockRegionSize) { Value = regionsSize };
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
                startBlockOffset: transactionFilePm.ReadUInt(TransactionFileOffset.AddBlockStartBlockOffset),
                regionsQtty: transactionFilePm.ReadByte(TransactionFileOffset.AddBlockRegionsQtty),
                regionsSize: transactionFilePm.ReadUInt(TransactionFileOffset.AddBlockRegionSize))
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.AddBlockCommitByte, (CommitState)transactionFilePm.ReadByte(TransactionFileOffset.AddBlockCommitByte)),
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PAllocator pAllocator)
        {
            var block = new PersistentBlockLayout(RegionSize.Value, RegionsQtty.Value)
            {
                BlockOffset = StartBlockOffset.Value
            };
            pAllocator.WriteBlockLayout(StartBlockOffset.Value, block);

            CommitByte.State = CommitState.CommitedAndWriteOnOriginalFileFinished;
            transactionFile.WriteByte(CommitByte.Value, offset: TransactionFileOffset.AddBlockCommitByte);
        }

        public void WriteTo(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            pmCSharpDefinedTypes.WriteUInt(StartBlockOffset.Value, offset: TransactionFileOffset.AddBlockStartBlockOffset);
            pmCSharpDefinedTypes.WriteByte(RegionsQtty.Value, offset: TransactionFileOffset.AddBlockRegionsQtty);
            pmCSharpDefinedTypes.WriteUInt(RegionSize.Value, offset: TransactionFileOffset.AddBlockRegionSize);

            // Commit byte need always be the last
            CommitByte.Commit();
            pmCSharpDefinedTypes.WriteByte(CommitByte.Value, offset: TransactionFileOffset.AddBlockCommitByte);
        }
    }
}
