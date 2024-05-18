namespace PM.AutomaticManager
{
    internal class MetaDataStructure
    {
        public MetadataType MetadataType { get; set; }
        public ObjectMetaDataStructure? ObjectMetaDataStructure { get; set; }
        public TransactionMetaDataStructure? TransactionMetaDataStructure { get; set; }
    }
}
