namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    internal class WrapperBlockLayouts : IComparable<WrapperBlockLayouts>
    {
        public BlockLayoutType BlockLayoutType { get; }
        public IBlockLayout Object { get; }

        public WrapperBlockLayouts(AddBlockLayout addBlockLayout)
        {
            BlockLayoutType = BlockLayoutType.AddBlock;
            Object = addBlockLayout;
        }

        public WrapperBlockLayouts(RemoveBlockLayout removeBlockLayout)
        {
            BlockLayoutType = BlockLayoutType.RemoveBlock;
            Object = removeBlockLayout;
        }

        public WrapperBlockLayouts(UpdateContentBlockLayout updateContentLayout)
        {
            BlockLayoutType = BlockLayoutType.UpdateContentBlock;
            Object = updateContentLayout;
        }

        public override int GetHashCode()
        {
            if (Object is null) 
                throw new ApplicationException("Object cannot be null");

            return Object.Order.Value;
        }

        public int CompareTo(WrapperBlockLayouts? other)
        {
            return Object.Order.Value - other.Object.Order.Value;
        }
    }
}
