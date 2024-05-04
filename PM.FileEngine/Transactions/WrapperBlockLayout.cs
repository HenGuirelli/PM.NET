namespace PM.FileEngine.Transactions
{
    internal class WrapperBlockLayout
    {
        public BlockLayoutType BlockLayoutType { get; }
        public IBlockLayout Object { get; }

        public WrapperBlockLayout(AddBlockLayout addBlockLayout)
        {
            BlockLayoutType = BlockLayoutType.AddBlock;
            Object = addBlockLayout;
        }

        public WrapperBlockLayout(RemoveBlockLayout removeBlockLayout)
        {
            BlockLayoutType = BlockLayoutType.RemoveBlock;
            Object = removeBlockLayout;
        }

        public WrapperBlockLayout(UpdateContentBlockLayout updateContentLayout)
        {
            BlockLayoutType = BlockLayoutType.UpdateContentBlock;
            Object = updateContentLayout;
        }
    }
}
