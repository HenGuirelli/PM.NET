using PM.Core.PMemory;
using System.Text;

namespace PM.AutomaticManager.MetaDatas
{
    internal class ObjectMetaDataStructure : MetadataStructure, IObjectReference
    {
        public override MetadataType Type => MetadataType.Object;
        public override int Size => 13 + (ObjectUserID.Length + 1) + (ClassTypeName.Length + 1) + (AssemblyFullName.Length + 1);

        public uint ObjectSize { get; set; }
        public string ObjectUserID { get; set; } = string.Empty;
        public string ClassTypeName { get; set; } = string.Empty;
        public string AssemblyFullName { get; set; } = string.Empty;


        protected override void ReadFrom(PersistentRegion metadataRegion)
        {
            base.ReadFrom(metadataRegion);
            ObjectSize = BitConverter.ToUInt32(metadataRegion.Read(count: sizeof(uint), offset: InternalOffset));
            InternalOffset += sizeof(uint);
            ObjectUserID = ReadString(metadataRegion);
            ClassTypeName = ReadString(metadataRegion);
            AssemblyFullName = ReadString(metadataRegion);
        }

        private string ReadString(PersistentRegion metadataRegion)
        {
            var stringBytes = new List<byte>();
            while (true)
            {
                var @byte = metadataRegion.Read(count: sizeof(byte), offset: InternalOffset)[0];

                if (@byte == 0)
                {
                    InternalOffset += sizeof(byte);
                    break;
                }

                stringBytes.Add(@byte);
                InternalOffset += sizeof(byte);
            }
            return Encoding.UTF8.GetString(stringBytes.ToArray());
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
            var buffer = new byte[Size];
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
            var idBytes = StringToByteArray(ObjectUserID);
            Array.Copy(sourceArray: idBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: idBytes.Length);
            bufferOffset += idBytes.Length;
            var assemblyFullNameBytes = StringToByteArray(AssemblyFullName);
            Array.Copy(sourceArray: assemblyFullNameBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: assemblyFullNameBytes.Length);
            bufferOffset += assemblyFullNameBytes.Length;
            var classTypeBytes = StringToByteArray(ClassTypeName);
            Array.Copy(sourceArray: classTypeBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: classTypeBytes.Length);
            bufferOffset += classTypeBytes.Length;
            return buffer;
        }

        private byte[] StringToByteArray(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (!value.EndsWith('\0'))
            {
                return bytes.Concat(new byte[] { 0 }).ToArray();
            }
            return bytes;
        }
    }
}
