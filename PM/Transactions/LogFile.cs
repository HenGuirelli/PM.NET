using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;
using System.Buffers.Binary;

namespace PM.Transactions
{
    public class LogFile
    {
        /// <summary>
        /// Item1: Offset of original file
        /// Item2: Type ID
        /// Item3: Content
        /// </summary>
        public List<(int, int, byte[])> LogFileContent { get; } = new List<(int, int, byte[])>();
        public bool IsCommitedLogFile { get; private set; }
        public string OriginalFileName { get; private set; }

        private readonly PmCSharpDefinedTypes _pmCSharpDefinedTypes;
        private readonly PointersToPersistentObjects _pointersToPersistentObjects;
        private int _internalOffset;

        public const int CommitByte = -1;

        public LogFile(PmCSharpDefinedTypes pmCSharpDefinedTypes)
        {
            _pmCSharpDefinedTypes = pmCSharpDefinedTypes;
            _pointersToPersistentObjects = new PointersToPersistentObjects();
            InternalReadLogFileWithoutThrowException();
        }

        private void InternalReadLogFileWithoutThrowException()
        {
            try
            {
                ReadLogFile();
            }
            catch (Exception ex)
            {
            }
        }

        private void ReadLogFile()
        {
            // Transaction finished successfully
            if (_pmCSharpDefinedTypes.ReadInt() == CommitByte)
            {
                IsCommitedLogFile = true;
            }

            bool finished = false;
            var offset = 0;
            while (true)
            {
                OriginalFileName = _pmCSharpDefinedTypes.ReadString(sizeof(int));
                offset += (OriginalFileName.Length * sizeof(char)) + sizeof(int);
                var propertyOffset = _pmCSharpDefinedTypes.ReadInt(offset);
                offset += 4;
                var propertyID = _pmCSharpDefinedTypes.ReadInt(offset);

                // Check if is in invalid state
                if (!SupportedTypesTable.Instance.IsValidID(propertyID) ||
                    propertyOffset < CommitByte ||
                    propertyOffset == 0)
                {
                    return;
                }

                offset += 4;
                var pmType = SupportedTypesTable.Instance.GetPmemType(propertyID);
                var content = _pmCSharpDefinedTypes.ReadBytes(pmType.SizeBytes, offset);
                offset += pmType.SizeBytes;
                LogFileContent.Add((propertyOffset, propertyID, content));
            }
        }

        public void Commit()
        {
            _pmCSharpDefinedTypes.WriteInt(CommitByte);
        }

        public void RollBack()
        {
            DeleteFile();
        }

        private void AddOffset() => _internalOffset += 4;

        internal void DeleteFile()
        {
            _pmCSharpDefinedTypes.DeleteFile();
        }

        public void WriteInt(int offset, int value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(int));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteInt(value, _internalOffset);

            var array = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(array, value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteByte(int offset, byte value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(byte));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteByte(value, _internalOffset);
            LogFileContent.Add((offset, pmType.ID, new[] { value }));
        }

        internal void WriteSByte(int offset, sbyte value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(sbyte));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteSByte(value, _internalOffset);
            LogFileContent.Add((offset, pmType.ID, new[] { (byte)value }));
        }

        internal void WriteShort(int offset, short value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(short));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteInt(value, _internalOffset);

            var array = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(array, value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteUShort(int offset, ushort value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(ushort));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteInt(value, _internalOffset);

            var array = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(array, value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteUInt(int offset, uint value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(uint));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteUInt(value, _internalOffset);

            var array = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(array, value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteLong(int offset, long value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(long));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteLong(value, _internalOffset);

            var array = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(array, value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteULong(int offset, ulong value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(ulong));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteULong(value, _internalOffset);

            var array = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(array, value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteFloat(int offset, float value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(float));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteFloat(value, _internalOffset);

            var array = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(array, value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteDouble(int offset, double value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(double));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteDouble(value, _internalOffset);

            var array = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(array, value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteDecimal(int offset, decimal value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(decimal));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteDecimal(value, _internalOffset);

            var bits = decimal.GetBits(value);
            var array = new byte[sizeof(decimal)];
            int count = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                var intBits = BitConverter.GetBytes(bits[i]);
                for (int j = 0; j < intBits.Length; j++)
                {
                    array[count++] = intBits[j];
                }
            }
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteChar(int offset, char value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(char));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteChar(value, _internalOffset);

            var array = new byte[sizeof(char)];
            BinaryPrimitives.WriteInt16BigEndian(array, (short)value);
            LogFileContent.Add((offset, pmType.ID, array));
        }

        internal void WriteBool(int offset, bool value)
        {
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(bool));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteBool(value, _internalOffset);
            // We use the BitConverter class while BinaryPrimitives does not support this type.
            LogFileContent.Add((offset, pmType.ID, BitConverter.GetBytes(value)));
        }

        internal void WriteString(int offset, string value)
        {
            var pointer = _pointersToPersistentObjects.GetNext();
            _pmCSharpDefinedTypes.WriteInt(offset, _internalOffset);
            AddOffset();
            var pmType = SupportedTypesTable.Instance.GetPmType(typeof(string));
            _pmCSharpDefinedTypes.WriteInt(pmType.ID, _internalOffset);
            AddOffset();
            _pmCSharpDefinedTypes.WriteULong(pointer, _internalOffset);

            var array = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(array, pointer);
            LogFileContent.Add((offset, pmType.ID, array));

            var pm = PmFactory.CreatePm(
                new PmMemoryMappedFileConfig(
                    name: Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString()),
                    size: sizeof(char) * (value.Length + 1)));
            var stringPmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm);
            stringPmCSharpDefinedTypes.WriteString(value);
        }

        internal void WriteOriginalFileName(string value)
        {
            _pmCSharpDefinedTypes.WriteString(value, offset: sizeof(int));
            _internalOffset += (value.Length * sizeof(char)) + sizeof(int);
        }

        internal string ReadOriginalFileName()
        {
            return _pmCSharpDefinedTypes.ReadString(offset: sizeof(int));
        }
    }
}
