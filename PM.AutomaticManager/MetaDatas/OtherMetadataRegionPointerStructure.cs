using PM.Core.PMemory;

namespace PM.AutomaticManager.MetaDatas
{
    internal class OtherMetadataRegionPointerStructure : MetadataStructure
    {
        public const uint SizeBytes = 7;
        public override MetadataType Type => MetadataType.OtherMetadataRegionPointer;
        public override uint Size => SizeBytes;

        internal override void WriteTo(PersistentRegion metadataRegion, int offset)
        {
            var buffer = GetBytes();
            metadataRegion.Write(buffer, offset);
        }

        private byte[] GetBytes()
        {
            var blockId = BitConverter.GetBytes(BlockID);
            var regionIndex = RegionIndex;

            var buffer = new byte[Size];
            var bufferOffset = 0;
            buffer[bufferOffset] = (byte)MetadataType.OtherMetadataRegionPointer;
            bufferOffset += sizeof(byte);
            buffer[bufferOffset] = BitConverter.GetBytes(true)[0];
            bufferOffset += sizeof(byte);
            Array.Copy(sourceArray: blockId, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: blockId.Length);
            bufferOffset += sizeof(uint);
            buffer[bufferOffset] = regionIndex;
            bufferOffset += sizeof(byte);
            return buffer;
        }
    }
}
