using PM.Core.PMemory;
using System.Text;

namespace PM.AutomaticManager.MetaDatas
{
    internal class ObjectMetaDataStructure : MetadataStructure, IObjectReference
    {
        public override MetadataType Type => MetadataType.Object;
        public override int Size => 13 + ObjectUserID.Length;

        public uint ObjectSize { get; set; }
        private string _objectUserID = string.Empty;
        public string ObjectUserID
        {
            get => _objectUserID;
            set
            {
                _objectUserID = value.EndsWith('\0') ? value : value + '\0';
            }
        }


        protected override void ReadFrom(PersistentRegion metadataRegion, int offset)
        {
            base.ReadFrom(metadataRegion, offset);
            ObjectSize = BitConverter.ToUInt32(metadataRegion.Read(count: sizeof(uint), offset: InternalOffset));
            InternalOffset += sizeof(uint);
            var stringBytes = new List<byte>();
            while (true)
            {
                var @byte = metadataRegion.Read(count: sizeof(byte), offset: InternalOffset)[0];

                if (@byte == 0) break;

                stringBytes.Add(@byte);
                InternalOffset += sizeof(byte);
            }

            ObjectUserID = Encoding.UTF8.GetString(stringBytes.ToArray());
        }

        internal override void WriteTo(PersistentRegion metadataRegion, int offset)
        {
            var buffer = GetBytes();
            metadataRegion.Write(buffer, offset);
        }

        private byte[] GetBytes()
        {
            var blockId = BitConverter.GetBytes(BlockID);
            var regionIndex = RegionIndex;
            var offsetInnerRegionBytes = BitConverter.GetBytes(OffsetInnerRegion); // Always zero
            var objectSizeBytes = BitConverter.GetBytes(ObjectSize);

            // 12 = metadata size + object user id length + \0 string byte
            var buffer = new byte[13 + ObjectUserID.Length + 1];
            var bufferOffset = 0;
            buffer[bufferOffset] = (byte)MetadataType.Object;
            bufferOffset += sizeof(byte);
            buffer[bufferOffset] = BitConverter.GetBytes(true)[0];
            bufferOffset += sizeof(byte);
            Array.Copy(sourceArray: blockId, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: blockId.Length);
            bufferOffset += sizeof(uint);
            buffer[bufferOffset] = regionIndex;
            bufferOffset += sizeof(byte);
            Array.Copy(sourceArray: offsetInnerRegionBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: offsetInnerRegionBytes.Length);
            bufferOffset += sizeof(ushort);
            Array.Copy(sourceArray: objectSizeBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: objectSizeBytes.Length);
            bufferOffset += sizeof(uint);
            var idBytes = Encoding.UTF8.GetBytes(ObjectUserID);
            Array.Copy(sourceArray: idBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: idBytes.Length);

            return buffer;
        }
    }
}
