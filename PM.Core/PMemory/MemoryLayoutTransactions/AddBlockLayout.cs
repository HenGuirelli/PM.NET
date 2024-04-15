using PM.Core.PMemory.FileFields;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    public class AddBlockLayout
    {
        public class Offset
        {
            public const int CommitByte = 0;
            public const int Order = 1;
            public const int BlockOffset = 3;
            public const int RegionsQtty = 7;
            public const int RegionSize = 8;
        }

        public const int Size = 10;

        public CommitByteField CommitByte
        {
            get => _commitByte;
            set
            {
                value.Offset = Offset.CommitByte;
                _commitByte = value;
            }
        }
        private CommitByteField _commitByte = new(Offset.CommitByte);

        public OrderField Order
        {
            get => _order;
            set
            {
                value.Offset = Offset.Order;
                _order = value;
            }
        }
        private OrderField _order = new(Offset.Order);

        public UInt32 BlockOffset { get; set; }
        public byte RegionsQtty { get; set; }
        public UInt32 RegionSize { get; set; }
    }
}
