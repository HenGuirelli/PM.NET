using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;

namespace PM.Core
{
    public class PmCSharpDefinedTypes : IDisposable
    {
        private readonly Stream _pm2;
        const string ReadErrorExceptionMessage = "Error on read from Stream";

        public PmCSharpDefinedTypes(Stream pm)
        {
            _pm2 = pm ?? throw new ArgumentNullException(nameof(pm));
        }

        public void Flush()
        {
            _pm2.Flush();
        }

        #region Char
        public char ReadChar(int offset = 0)
        {
            var array = new byte[sizeof(char)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(char)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return (char)BinaryPrimitives.ReadInt16BigEndian(array);
        }

        public void WriteChar(char value, int offset = 0)
        {
            var array = new byte[sizeof(char)];
            BinaryPrimitives.WriteInt16BigEndian(array, (short)value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(char));
        }
        #endregion

        #region Decimal
        public decimal ReadDecimal(int offset = 0)
        {
            var array = new byte[sizeof(decimal)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(decimal)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            var i1 = BitConverter.ToInt32(array, 0);
            var i2 = BitConverter.ToInt32(array, 4);
            var i3 = BitConverter.ToInt32(array, 8);
            var i4 = BitConverter.ToInt32(array, 12);
            return new decimal(new int[] { i1, i2, i3, i4 });
        }

        public void WriteDecimal(decimal value, int offset = 0)
        {
            _pm2.Seek(offset, SeekOrigin.Begin);
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
            _pm2.Write(decimalResult, 0, sizeof(decimal));
        }
        #endregion

        #region Double
        public double ReadDouble(int offset = 0)
        {
            var array = new byte[sizeof(double)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(double)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadDoubleBigEndian(array);
        }

        public void WriteDouble(double value, int offset = 0)
        {
            var array = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(array, value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(double));
        }
        #endregion

        #region Float
        public float ReadFloat(int offset = 0)
        {
            var array = new byte[sizeof(float)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(float)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadSingleBigEndian(array);
        }

        public void WriteFloat(float value, int offset = 0)
        {
            var array = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(array, value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(float));
        }
        #endregion

        #region Long
        public ulong ReadULong(int offset = 0)
        {
            var array = new byte[sizeof(ulong)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(ulong)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadUInt64BigEndian(array);
        }

        public void WriteULong(ulong value, int offset = 0)
        {
            var array = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(array, value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(ulong));
        }
        #endregion

        #region Long
        public long ReadLong(int offset = 0)
        {
            var array = new byte[sizeof(long)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(long)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadInt64BigEndian(array);
        }

        public void WriteLong(long value, int offset = 0)
        {
            var array = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(array, value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(long));
        }
        #endregion

        #region UInt
        public uint ReadUInt(int offset = 0)
        {
            var array = new byte[sizeof(uint)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(uint)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadUInt32BigEndian(array);
        }

        public void WriteUInt(uint value, int offset = 0)
        {
            var array = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(array, value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(uint));
        }
        #endregion

        #region UShort
        public ushort ReadUShort(int offset = 0)
        {
            var array = new byte[sizeof(ushort)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(ushort)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadUInt16BigEndian(array);
        }

        public void WriteUShort(ushort value, int offset = 0)
        {
            var array = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(array, value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(ushort));
        }
        #endregion

        #region Short
        public short ReadShort(int offset = 0)
        {
            var array = new byte[sizeof(short)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(short)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadInt16BigEndian(array);
        }

        public void WriteShort(short value, int offset = 0)
        {
            var array = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(array, value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(short));
        }
        #endregion

        #region SByte
        public sbyte ReadSByte(int offset = 0)
        {
            _pm2.Seek(offset, SeekOrigin.Begin);
            return (sbyte)_pm2.ReadByte();
        }

        public void WriteSByte(sbyte value, int offset = 0)
        {
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.WriteByte((byte)value);
        }
        #endregion 

        #region Byte
        public byte ReadByte(int offset = 0)
        {
            _pm2.Seek(offset, SeekOrigin.Begin);
            return (byte)_pm2.ReadByte();
        }

        public void WriteByte(byte value, int offset = 0)
        {
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.WriteByte(value);
        }

        public byte[] ReadBytes(int count, int offset = 0)
        {
            var array = new byte[count];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, count) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return array;
        }

        public void WriteBytes(byte[] value, int offset = 0)
        {
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(value);
        }
        #endregion 

        #region Bool
        // We use the BitConverter class while BinaryPrimitives does not support this type.
        public bool ReadBool(int offset = 0)
        {
            var array = new byte[sizeof(bool)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(bool)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BitConverter.ToBoolean(array);
        }

        // We use the BitConverter class while BinaryPrimitives does not support this type.
        public void WriteBool(bool value, int offset = 0)
        {
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(BitConverter.GetBytes(value));
        }
        #endregion 

        #region Int32
        public int ReadInt(int offset = 0)
        {
            var array = new byte[sizeof(int)];
            _pm2.Seek(offset, SeekOrigin.Begin);
            if (_pm2.Read(array, 0, sizeof(int)) == 0)
            {
                throw new ApplicationException(ReadErrorExceptionMessage);
            }
            return BinaryPrimitives.ReadInt32BigEndian(array);
        }

        public void WriteInt(int value, int offset = 0)
        {
            var array = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(array, value);
            _pm2.Seek(offset, SeekOrigin.Begin);
            _pm2.Write(array, 0, sizeof(int));
        }
        #endregion

        #region String
        public string ReadString(int offset = 0)
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

        public void WriteString(string value, int offset = 0)
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
            _pm2.Dispose();
        }
    }
}
