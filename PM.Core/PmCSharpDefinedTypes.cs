using System.Buffers.Binary;
using System.Text;

namespace PM.Core
{
    public class PmCSharpDefinedTypes
    {
        private readonly IPm _pm;

        public PmCSharpDefinedTypes(IPm pm)
        {
            _pm = pm;
        }

        #region Char
        public char ReadChar(int offset = 0)
        {
            var array = _pm.Load(sizeof(char), offset);
            return (char)BinaryPrimitives.ReadInt16BigEndian(array);
        }

        public void WriteChar(char value, int offset = 0)
        {
            var array = new byte[sizeof(char)];
            BinaryPrimitives.WriteInt16BigEndian(array, (short)value);
            _pm.Store(array, offset);
        }
        #endregion

        #region Decimal
        public decimal ReadDecimal(int offset = 0)
        {
            var array = _pm.Load(sizeof(decimal), offset);
            var i1 = BitConverter.ToInt32(array, 0);
            var i2 = BitConverter.ToInt32(array, 4);
            var i3 = BitConverter.ToInt32(array, 8);
            var i4 = BitConverter.ToInt32(array, 12);
            return new decimal(new int[] { i1, i2, i3, i4 });
        }

        public void WriteDecimal(decimal value, int offset = 0)
        {
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
            _pm.Store(decimalResult, offset);
        }
        #endregion

        #region Double
        public double ReadDouble(int offset = 0)
        {
            var array = _pm.Load(sizeof(double), offset);
            return BinaryPrimitives.ReadDoubleBigEndian(array);
        }

        public void WriteDouble(double value, int offset = 0)
        {
            var array = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(array, value);
            _pm.Store(array, offset);
        }
        #endregion

        #region Float
        public float ReadFloat(int offset = 0)
        {
            var array = _pm.Load(sizeof(float), offset);
            return BinaryPrimitives.ReadSingleBigEndian(array);
        }

        public void WriteFloat(float value, int offset = 0)
        {
            var array = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(array, value);
            _pm.Store(array, offset);
        }
        #endregion

        #region Long
        public ulong ReadULong(int offset = 0)
        {
            var array = _pm.Load(sizeof(ulong), offset);
            return BinaryPrimitives.ReadUInt64BigEndian(array);
        }

        public void WriteULong(ulong value, int offset = 0)
        {
            var array = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(array, value);
            _pm.Store(array, offset);
        }
        #endregion

        #region Long
        public long ReadLong(int offset = 0)
        {
            var array = _pm.Load(sizeof(long), offset);
            return BinaryPrimitives.ReadInt64BigEndian(array);
        }

        public void WriteLong(long value, int offset = 0)
        {
            var array = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(array, value);
            _pm.Store(array, offset);
        }
        #endregion

        #region UInt
        public uint ReadUInt(int offset = 0)
        {
            var array = _pm.Load(sizeof(uint), offset);
            return BinaryPrimitives.ReadUInt32BigEndian(array);
        }

        public void WriteUInt(uint value, int offset = 0)
        {
            var array = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(array, value);
            _pm.Store(array, offset);
        }
        #endregion

        #region UShort
        public ushort ReadUShort(int offset = 0)
        {
            var array = _pm.Load(sizeof(ushort), offset);
            return BinaryPrimitives.ReadUInt16BigEndian(array);
        }

        public void WriteUShort(ushort value, int offset = 0)
        {
            var array = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(array, value);
            _pm.Store(array, offset);
        }
        #endregion

        #region Short
        public short ReadShort(int offset = 0)
        {
            var array = _pm.Load(sizeof(short), offset);
            return BinaryPrimitives.ReadInt16BigEndian(array);
        }

        public void WriteShort(short value, int offset = 0)
        {
            var array = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(array, value);
            _pm.Store(array, offset);
        }
        #endregion

        #region SByte
        public sbyte ReadSByte(int offset = 0)
        {
            return (sbyte)_pm.Load(offset);
        }

        public void WriteSByte(sbyte value, int offset = 0)
        {
            _pm.Store((byte)value, offset);
        }
        #endregion 

        #region Byte
        public byte ReadByte(int offset = 0)
        {
            return _pm.Load(offset);
        }

        public void WriteByte(byte value, int offset = 0)
        {
            _pm.Store(value, offset);
        }

        public byte[] ReadBytes(int count, int offset = 0)
        {
            return _pm.Load(count, offset);
        }

        public void WriteBytes(byte[] value, int offset = 0)
        {
            _pm.Store(value, offset);
        }
        #endregion 

        #region Bool
        // We use the BitConverter class while BinaryPrimitives does not support this type.
        public bool ReadBool(int offset = 0)
        {
            var array = _pm.Load(sizeof(bool), offset);
            return BitConverter.ToBoolean(array);
        }

        // We use the BitConverter class while BinaryPrimitives does not support this type.
        public void WriteBool(bool value, int offset = 0)
        {
            _pm.Store(BitConverter.GetBytes(value), offset);
        }
        #endregion 

        #region Int32
        public int ReadInt(int offset = 0)
        {
            var array = _pm.Load(sizeof(int), offset);
            return BinaryPrimitives.ReadInt32BigEndian(array);
        }

        public void WriteInt(int value, int offset = 0)
        {
            var array = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(array, value);
            _pm.Store(array, offset);
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

        public void DeleteFile()
        {
            _pm.DeleteFile();
        }

        public void Lock()
        {
            _pm.Lock();
        }

        public void Release()
        {
            _pm.Release();
        }
    }
}
