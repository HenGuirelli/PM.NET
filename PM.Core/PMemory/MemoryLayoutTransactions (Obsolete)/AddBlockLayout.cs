using PM.Core.PMemory.FileFields;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{

    public class AddBlockLayout : IBlockLayout
    {
        public class Offset
        {
            public const int CommitByte = 0;
            public const int Order = 1;
            public const int BlockOffset = 3;
            public const int RegionsQtty = 7;
            public const int RegionSize = 8;
        }

        public const int Size = 12;

        public long TransactionOffset { get; set; }

        public CommitByteField CommitByte
        {
            get => _commitByte;
            internal set
            {
                value.Offset = Offset.CommitByte;
                _commitByte = value;
            }
        }
        private CommitByteField _commitByte = new(Offset.CommitByte);

        public OrderField Order
        {
            get => _order ??= new OrderField(Offset.Order, instance: 1);
            internal set
            {
                if (value != null)
                {
                    value.Offset = Offset.Order;
                    _order = value;
                }
            }
        }
        private OrderField? _order;

        public UInt32 BlockOffset { get; set; }
        public byte RegionsQtty { get; set; }
        public UInt32 RegionSize { get; set; }
    }
}
