﻿using PM.Core.PMemory.FileFields;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    public class RemoveBlockLayout : IBlockLayout
    {
        public const int Size = 5;

        public class Offset
        {
            public const int CommitByte = 0;
            public const int Order = 1;
        }

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
    }
}
