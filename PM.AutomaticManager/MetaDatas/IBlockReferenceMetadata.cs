namespace PM.AutomaticManager.MetaDatas
{
    internal interface IBlockReferenceMetadata
    {
        UInt32 BlockID { get; }
        byte RegionIndex { get; }
        UInt16 OffsetInnerRegion { get; }
    }
}
