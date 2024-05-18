using PM.AutomaticManager.Tansactions;
using PM.Core.PMemory;

namespace PM.AutomaticManager
{
    internal class TransactionMetaDataStructure
    {
        private PersistentRegion _metadataRegion;
        private uint _initialMetadatastructureOffset;

        public UInt32 BlockID { get; set; }
        public byte RegionIndex { get; set; }
        public UInt16 OffsetInnerRegion { get; set; }
        public UInt32 ObjectSize { get; set; }
        public TransactionState TransactionState { get; set; }
        public UInt32 TransactionblockIDTarget { get; set; }
        public byte TransactionRegionIndexTarget { get; set; }

        public TransactionMetaDataStructure(
            PersistentRegion metadataRegion,
            uint initialMetadatastructureOffset)
        {
            _metadataRegion = metadataRegion;
            _initialMetadatastructureOffset = initialMetadatastructureOffset;
        }

        internal void Invalidate()
        {
            _metadataRegion.Write(BitConverter.GetBytes(false), offset: (int)_initialMetadatastructureOffset + 1);
        }

        internal void ChangeState(TransactionState value)
        {
            _metadataRegion.Write(new byte[] { (byte)value }, offset: (int)_initialMetadatastructureOffset + 13);
            TransactionState = value;
        }
    }
}
