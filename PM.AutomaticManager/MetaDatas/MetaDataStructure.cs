using PM.Core.PMemory;

namespace PM.AutomaticManager.MetaDatas
{
    internal abstract class MetadataStructure : IBlockReferenceMetadata
    {
        protected int InternalOffset { get; set; }
        public abstract int Size { get; }

        public abstract MetadataType Type { get; }
        public bool IsValid { get; set; }

        public UInt32 BlockID { get; set; }
        public byte RegionIndex { get; set; }
        public UInt16 OffsetInnerRegion { get; set; }

        protected virtual void ReadFrom(PersistentRegion metadataRegion)
        {
            InternalOffset += sizeof(byte); // Skip MetadataType

            IsValid = Convert.ToBoolean(metadataRegion.Read(count: sizeof(byte), offset: InternalOffset)[0]);
            InternalOffset += sizeof(byte);
            BlockID = BitConverter.ToUInt32(metadataRegion.Read(count: sizeof(uint), offset: InternalOffset));
            InternalOffset += sizeof(uint);
            RegionIndex = metadataRegion.Read(count: sizeof(byte), offset: InternalOffset)[0];
            InternalOffset += sizeof(byte);
            OffsetInnerRegion = BitConverter.ToUInt16(metadataRegion.Read(count: sizeof(ushort), offset: InternalOffset));
            InternalOffset += sizeof(ushort);
        }

        public static MetadataStructure CreateFrom(PersistentRegion metadataRegion, int offset)
        {
            var metadataType = metadataRegion.Read(count: 1, offset: offset)[0];
            var resultObj = CreateMetadataStructureObject(offset, (MetadataType)metadataType);
            resultObj.ReadFrom(metadataRegion);
            return resultObj;
        }

        private static MetadataStructure CreateMetadataStructureObject(int offset, MetadataType metadataType)
        {
            switch (metadataType)
            {
                case MetadataType.Object:
                    return new ObjectMetaDataStructure()
                    {
                        InternalOffset = offset
                    };
                case MetadataType.Transaction:
                    return new TransactionMetaDataStructure()
                    {
                        InternalOffset = offset
                    };
                default:
                    throw new NotImplementedException();
            }
        }

        internal abstract void WriteTo(PersistentRegion metadataRegion, int offset);
    }
}
