namespace PM.AutomaticManager
{
    internal class ObjectMetaDataStructure
    {
        public UInt32 BlockID { get; set; }
        public byte RegionIndex { get; set; }
        public UInt16 OffsetInnerRegion { get; set; }
        public UInt32 ObjectSize { get; set; }
        public string? ObjectUserName { get; set; }
    }
}
