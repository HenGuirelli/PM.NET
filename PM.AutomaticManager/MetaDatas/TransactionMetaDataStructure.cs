using PM.AutomaticManager.Tansactions;
using PM.Core.PMemory;

namespace PM.AutomaticManager.MetaDatas
{
    internal class TransactionMetaDataStructure : MetadataStructure, IObjectReference
    {
        public override MetadataType Type => MetadataType.Transaction;
        public override int Size => 19;

        public uint ObjectSize { get; set; }
        public TransactionState TransactionState { get; set; }
        public uint TransactionBlockIDTarget { get; set; }
        public byte TransactionRegionIndexTarget { get; set; }

        public int? InitialOffset { get; set; }
        public PersistentRegion? MetadataRegion { get; set; }

        protected override void ReadFrom(PersistentRegion metadataRegion)
        {
            //InitialOffset = offset;
            MetadataRegion = metadataRegion;

            base.ReadFrom(metadataRegion);
            TransactionState = (TransactionState)metadataRegion.Read(count: sizeof(byte), offset: InternalOffset)[0];
            InternalOffset += sizeof(byte);
            TransactionBlockIDTarget = BitConverter.ToUInt32(metadataRegion.Read(count: sizeof(uint), offset: InternalOffset));
            InternalOffset += sizeof(uint);
            TransactionRegionIndexTarget = metadataRegion.Read(count: sizeof(byte), offset: InternalOffset)[0];
            InternalOffset += sizeof(byte);
        }

        internal void ChangeState(TransactionState value)
        {
            if (InitialOffset is null)
            {
                throw new ApplicationException($"{nameof(InitialOffset)} cannot be null, please set the property");
            }

            if (MetadataRegion is null)
            {
                throw new ApplicationException($"{nameof(MetadataRegion)} cannot be null, please set the property");
            }

            MetadataRegion.Write(new byte[] { (byte)value }, offset: (int)InitialOffset + 13);
            TransactionState = value;
        }

        internal void Invalidate()
        {
            if (InitialOffset is null)
            {
                throw new ApplicationException($"{nameof(InitialOffset)} cannot be null, please set the property");
            }

            if (MetadataRegion is null)
            {
                throw new ApplicationException($"{nameof(MetadataRegion)} cannot be null, please set the property");
            }

            MetadataRegion.Write(BitConverter.GetBytes(false), offset: (int)InitialOffset + 1);
        }

        internal override void WriteTo(PersistentRegion metadataRegion, int offset)
        {
            InitialOffset = offset;
            MetadataRegion = metadataRegion;
            var buffer = GetBytes();
            metadataRegion.Write(buffer, offset);
        }

        private byte[] GetBytes()
        {
            var buffer = new byte[19];
            var bufferOffset = 0;
            // MetadataType
            buffer[bufferOffset] = (byte)MetadataType.Transaction;
            bufferOffset += sizeof(byte);
            // Valid
            buffer[bufferOffset] = BitConverter.GetBytes(true)[0];
            bufferOffset += sizeof(byte);
            // BlockId
            var originalBlockId = BitConverter.GetBytes(BlockID);
            Array.Copy(sourceArray: originalBlockId, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: originalBlockId.Length);
            bufferOffset += sizeof(uint);
            // region index
            var originalRegionIndex = RegionIndex;
            buffer[bufferOffset] = originalRegionIndex;
            bufferOffset += sizeof(byte);
            // offsetInnerRegion
            var offsetInnerRegionBytes = BitConverter.GetBytes(OffsetInnerRegion);
            Array.Copy(sourceArray: offsetInnerRegionBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: offsetInnerRegionBytes.Length);
            bufferOffset += sizeof(ushort);
            // objectSize
            var objectSizeBytes = BitConverter.GetBytes(ObjectSize);
            Array.Copy(sourceArray: objectSizeBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: objectSizeBytes.Length);
            bufferOffset += sizeof(uint);
            // TansactionState
            byte transactionStateBytes = 0; // Init always 0
            buffer[bufferOffset] = transactionStateBytes;
            bufferOffset += sizeof(byte);
            // TransactionBlockIDTarget
            var transactionBlockIDTarget = BitConverter.GetBytes(TransactionBlockIDTarget);
            Array.Copy(sourceArray: transactionBlockIDTarget, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: transactionBlockIDTarget.Length);
            bufferOffset += sizeof(uint);
            // TransactionRegionIndexTarget
            buffer[bufferOffset] = TransactionRegionIndexTarget;
            bufferOffset += sizeof(byte);

            return buffer;
        }
    }
}
