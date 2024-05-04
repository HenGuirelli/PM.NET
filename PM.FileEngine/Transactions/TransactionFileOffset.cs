namespace PM.FileEngine.Transactions
{
    internal class TransactionFileOffset
    {
        public const int HeaderVersion = 0;
        public const int HeaderCommitByte = 1;

        public const int AddBlockCommitByte = 3;
        public const int AddBlockOrder = 4;
        public const int AddBlockStartBlockOffset = 6;
        public const int AddBlockRegionsQtty = 10;
        public const int AddBlockRegionSize = 11;

        public const int RemoveBlockCommitByte = 15;
        public const int RemoveBlockOrder = 16;
        public const int RemoveBlockBeforeBlockOffset = 18;
        public const int RemoveBlockRemovedBlockOffset = 22;
        public const int RemoveBlockAfterBlockOffset = 16;

        public const int UpdateContentBlockCommitByte = 22;
        public const int UpdateContentBlockOrder = 23;
        public const int UpdateContentBlockStartBlockOffset = 25;
        public const int UpdateContentBlockContentSize = 29;
        public const int UpdateContentBlockContent = 33;
    }
}
