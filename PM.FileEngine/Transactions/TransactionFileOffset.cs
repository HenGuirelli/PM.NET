namespace PM.FileEngine.Transactions
{
    public class TransactionFileOffset
    {
        public const int HeaderCommitByte = 0;
        public const int HeaderVersion = 1;
        public const int HeaderBlockType = 3;

        public const int AddBlockCommitByte = 4;
        public const int AddBlockStartBlockOffset = 5;
        public const int AddBlockRegionsQtty = 9;
        public const int AddBlockRegionSize = 10;

        public const int RemoveBlockCommitByte = 3;
        public const int BeforeBlockID = 5;
        public const int NextBlockID = 9;

        public const int UpdateContentBlockCommitByte = 4;
        public const int UpdateContentBlockStartBlockOffset = 5;
        public const int UpdateContentBlockContentSize = 9;
        public const int UpdateContentBlockContent = 13;

        public const int UpdateFreeBlocksFromBlockLayoutCommitByte = 4;
        public const int UpdateFreeBlocksFromBlockLayoutStartBlockOffset = 5;
        public const int UpdateFreeBlocksFromBlockLayoutFreeBlocksValue = 9;
    }
}
