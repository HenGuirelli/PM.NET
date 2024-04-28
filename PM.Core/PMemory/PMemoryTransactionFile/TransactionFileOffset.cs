namespace PM.Core.PMemory.PMemoryTransactionFile
{
    internal class TransactionFileOffset
    {
        public const int HeaderCommitByte = 0;
        public const int HeaderVersion = 1;

        public const int AddBlockCommitByte = 3;
        public const int AddBlockOrder = 4;
        public const int AddBlockStartBlockOffset = 6;
        public const int AddBlockRegionsQtty = 10;
        public const int AddBlockRegionSize = 11;

        public const int RemoveBlockCommitByte = 15;
        public const int RemoveBlockOrder = 16;
        public const int RemoveBlockStartBlockOffset = 18;

        public const int UpdateContentBlockCommitByte = 22;
        public const int UpdateContentBlockOrder = 23;
        public const int UpdateContentBlockStartBlockOffset = 25;
        public const int UpdateContentBlockRegionsQtty = 29;
        public const int UpdateContentBlockRegionSize = 33;
    }
}
