using System.Buffers.Binary;
using System.Text;

namespace PM.Core
{
    public class PmCSharpDefinedTypes : IDisposable
    {
        private readonly MemoryMappedFileBasedStream _pm;
        const string ReadErrorExceptionMessage = "Error on read from Stream";
        public string FilePath => _pm.FilePath;
        public MemoryMappedFileBasedStream FileBasedStream => _pm;

        public PmCSharpDefinedTypes(MemoryMappedFileBasedStream pm)
        {
            _pm = pm ?? throw new ArgumentNullException(nameof(pm));
        }

        public void Flush()
        {
            _pm.Flush();
        }

        #region Char
        public char ReadChar(long offset = 0)
        {
            var array = new byte[sizeof(char)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(char)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return (char)BinaryPrimitives.ReadInt16BigEndian(array);
        }

        public void WriteChar(char value, long offset = 0)
        {
            var array = new byte[sizeof(char)];
            BinaryPrimitives.WriteInt16BigEndian(array, (short)value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(char));
        }
        #endregion

        #region Decimal
        public decimal ReadDecimal(long offset = 0)
        {
            var array = new byte[sizeof(decimal)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(decimal)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            var i1 = BitConverter.ToInt32(array, 0);
            var i2 = BitConverter.ToInt32(array, 4);
            var i3 = BitConverter.ToInt32(array, 8);
            var i4 = BitConverter.ToInt32(array, 12);
            return new decimal(new int[] { i1, i2, i3, i4 });
        }

        public void WriteDecimal(decimal value, long offset = 0)
        {
            _pm.Seek(offset, SeekOrigin.Begin);
            var bits = decimal.GetBits(value);
            var decimalResult = new byte[sizeof(decimal)];
            int count = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                var intBits = BitConverter.GetBytes(bits[i]);
                for (int j = 0; j < intBits.Length; j++)
                {
                    decimalResult[count++] = intBits[j];
                }
            }
            _pm.Write(decimalResult, 0, sizeof(decimal));
        }
        #endregion

        #region Double
        public double ReadDouble(long offset = 0)
        {
            var array = new byte[sizeof(double)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(double)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadDoubleBigEndian(array);
        }

        public void WriteDouble(double value, long offset = 0)
        {
            var array = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(array, value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(double));
        }
        #endregion

        #region Float
        public float ReadFloat(long offset = 0)
        {
            var array = new byte[sizeof(float)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(float)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadSingleBigEndian(array);
        }

        public void WriteFloat(float value, long offset = 0)
        {
            var array = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(array, value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(float));
        }
        #endregion

        #region Long
        public ulong ReadULong(long offset = 0)
        {
            var array = new byte[sizeof(ulong)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(ulong)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadUInt64BigEndian(array);
        }

        public void WriteULong(ulong value, long offset = 0)
        {
            var array = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(array, value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(ulong));
        }
        #endregion

        #region Long
        public long ReadLong(long offset = 0)
        {
            var array = new byte[sizeof(long)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(long)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadInt64BigEndian(array);
        }

        public void WriteLong(long value, long offset = 0)
        {
            var array = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(array, value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(long));
        }
        #endregion

        #region UInt
        public uint ReadUInt(long offset = 0)
        {
            var array = new byte[sizeof(uint)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(uint)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadUInt32BigEndian(array);
        }

        public void WriteUInt(uint value, long offset = 0)
        {
            var array = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(array, value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(uint));
        }
        #endregion

        #region UShort
        public ushort ReadUShort(long offset = 0)
        {
            var array = new byte[sizeof(ushort)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(ushort)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadUInt16BigEndian(array);
        }

        public void WriteUShort(ushort value, long offset = 0)
        {
            var array = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(array, value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(ushort));
        }
        #endregion

        #region Short
        public short ReadShort(long offset = 0)
        {
            var array = new byte[sizeof(short)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(short)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadInt16BigEndian(array);
        }

        public void WriteShort(short value, long offset = 0)
        {
            var array = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(array, value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(short));
        }
        #endregion

        #region SByte
        public sbyte ReadSByte(long offset = 0)
        {
            _pm.Seek(offset, SeekOrigin.Begin);
            return (sbyte)_pm.ReadByte();
        }

        public void WriteSByte(sbyte value, long offset = 0)
        {
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.WriteByte((byte)value);
        }
        #endregion 

        #region Byte
        public byte ReadByte(long offset = 0)
        {
            _pm.Seek(offset, SeekOrigin.Begin);
            return (byte)_pm.ReadByte();
        }

        public void WriteByte(byte value, long offset = 0)
        {
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.WriteByte(value);
        }

        public byte[] ReadBytes(int count, long offset = 0)
        {
            var array = new byte[count];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, count) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return array;
        }

        public void WriteBytes(byte[] value, long offset = 0)
        {
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(value);
        }
        #endregion 

        #region Bool
        // We use the BitConverter class while BinaryPrimitives does not support this type.
        public bool ReadBool(long offset = 0)
        {
            var array = new byte[sizeof(bool)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(bool)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BitConverter.ToBoolean(array);
        }

        // We use the BitConverter class while BinaryPrimitives does not support this type.
        public void WriteBool(bool value, long offset = 0)
        {
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(BitConverter.GetBytes(value));
        }
        #endregion 

        #region Int32
        public int ReadInt(long offset = 0)
        {
            var array = new byte[sizeof(int)];
            _pm.Seek(offset, SeekOrigin.Begin);
            if (_pm.Read(array, 0, sizeof(int)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadInt32BigEndian(array);
        }

        public void WriteInt(int value, long offset = 0)
        {
            var array = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(array, value);
            _pm.Seek(offset, SeekOrigin.Begin);
            _pm.Write(array, 0, sizeof(int));
        }
        #endregion

        #region String
        public string ReadString(long offset = 0)
        {
            var stringBuilder = new StringBuilder();

            int i = 0;
            char letter;
            while ((letter = ReadChar(offset + (i * sizeof(char)))) != '\0')
            {
                stringBuilder.Append(letter);
                i++;
            }
            return stringBuilder.ToString();
        }

        public void WriteString(string value, long offset = 0)
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteChar(value[i], offset + (i * sizeof(char)));
            }
            WriteChar('\0', offset + (value.Length * sizeof(char)));
        }
        #endregion

        public void Dispose()
        {
            _pm.Dispose();
        }

        public void Delete()
        {
            _pm.Delete();
        }

        internal void Resize(long size)
        {
            _pm.Resize(size);
        }
    }
}
