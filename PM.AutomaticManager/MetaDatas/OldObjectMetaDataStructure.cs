namespace PM.AutomaticManager.MetaDatas
{
    internal class OldObjectMetaDataStructure
    {
        public uint BlockID { get; set; }
        public byte RegionIndex { get; set; }
        public ushort OffsetInnerRegion { get; set; }
        public uint ObjectSize { get; set; }
        public string? ObjectUserID { get; set; }
    }
}
