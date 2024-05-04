using PM.Common;
using PM.FileEngine.FileFields;

namespace PM.FileEngine.Transactions
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

        public StartBlockOffsetField StartBlockOffset { get; set; } = new StartBlockOffsetField(TransactionFileOffset.UpdateContentBlockStartBlockOffset);
        public ContentSizeField ContentSize { get; set; } = new ContentSizeField(TransactionFileOffset.UpdateContentBlockContentSize);
        public ContentField Content { get; set; } = new ContentField(TransactionFileOffset.UpdateContentBlockContent);

        /// <summary>
        /// Total length, inclusive headers and other blocks
        /// </summary>
        public long TotalLength => (Content?.Value?.Length ?? 0) + 29;

        public UpdateContentBlockLayout(uint startBlockOffset, uint contentSize, byte[] content)
        {
            StartBlockOffset = new StartBlockOffsetField(offset: TransactionFileOffset.UpdateContentBlockStartBlockOffset) { Value = startBlockOffset };
            ContentSize = new ContentSizeField(offset: TransactionFileOffset.UpdateContentBlockContentSize) { Value = contentSize };
            Content = new ContentField(offset: TransactionFileOffset.UpdateContentBlockContent) { Value = content };
        }

        public static bool TryLoadFromTransactionFile(PmCSharpDefinedTypes transactionFilePm, out UpdateContentBlockLayout? result)
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
            var commitByteState = (CommitState)transactionFilePm.ReadByte(TransactionFileOffset.UpdateContentBlockCommitByte);
            return commitByteState == CommitState.Commited;
        }

        public static UpdateContentBlockLayout LoadFromTransactionFile(PmCSharpDefinedTypes pmTransactionFile)
        {
            var contentSize = pmTransactionFile.ReadUShort(TransactionFileOffset.UpdateContentBlockContentSize);
            return new UpdateContentBlockLayout(
                pmTransactionFile.ReadUInt(TransactionFileOffset.UpdateContentBlockStartBlockOffset),
                contentSize,
                pmTransactionFile.ReadBytes(count: contentSize, offset: TransactionFileOffset.UpdateContentBlockContent))
            {
                CommitByte = new CommitByteField(offset: TransactionFileOffset.UpdateContentBlockCommitByte, (CommitState)pmTransactionFile.ReadByte(TransactionFileOffset.UpdateContentBlockCommitByte)),
            };
        }

        public void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PAllocator pAllocator)
        {
            throw new NotImplementedException();
        }

        public void WriteTo(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            pmCSharpDefinedTypes.WriteUInt(StartBlockOffset.Value, offset: TransactionFileOffset.UpdateContentBlockStartBlockOffset);
            pmCSharpDefinedTypes.WriteUInt(ContentSize.Value, offset: TransactionFileOffset.UpdateContentBlockContentSize);
            if (Content.Value != null)
            {
                pmCSharpDefinedTypes.WriteBytes(Content.Value, offset: TransactionFileOffset.UpdateContentBlockContent);
            }

            // Commit byte need always be the last
            CommitByte.Commit();
            pmCSharpDefinedTypes.WriteByte(CommitByte.Value, offset: TransactionFileOffset.UpdateContentBlockCommitByte);
        }
    }
}
