using System;
using System.IO;
using Xunit;

namespace PM.Core.Tests
{
    public class PmCSharpDefinedTypesTests
    {
        private static readonly Random _random = new();

        [Fact]
        public void OnWriteAndReadChar_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(
                nameof(OnWriteAndReadChar_ShouldExecWithoutException),
                sizeof(char)*2);
            pmPrimitives.WriteChar(char.MaxValue);
            Assert.Equal(char.MaxValue, pmPrimitives.ReadChar());
            pmPrimitives.WriteChar(char.MinValue);
            Assert.Equal(char.MinValue, pmPrimitives.ReadChar());

            pmPrimitives.WriteChar(char.MaxValue, offset: sizeof(char));
            Assert.Equal(char.MaxValue, pmPrimitives.ReadChar(sizeof(char)));

            pmPrimitives.WriteChar(char.MinValue, offset: sizeof(char));
            Assert.Equal(char.MinValue, pmPrimitives.ReadChar(sizeof(char)));
        }

        [Fact]
        public void OnWriteAndReadDecimal_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadDecimal_ShouldExecWithoutException));
            pmPrimitives.WriteDecimal(decimal.MaxValue);
            Assert.Equal(decimal.MaxValue, pmPrimitives.ReadDecimal());
            
            pmPrimitives.WriteDecimal(decimal.MinValue);
            Assert.Equal(decimal.MinValue, pmPrimitives.ReadDecimal());
        }
        
        [Fact]
        public void OnWriteAndReadDouble_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadDouble_ShouldExecWithoutException));
            pmPrimitives.WriteDouble(double.MaxValue);
            Assert.Equal(double.MaxValue, pmPrimitives.ReadDouble());
            
            pmPrimitives.WriteDouble(double.MinValue);
            Assert.Equal(double.MinValue, pmPrimitives.ReadDouble());
        }

        [Fact]
        public void OnWriteAndReadFloat_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadFloat_ShouldExecWithoutException));
            pmPrimitives.WriteFloat(float.MaxValue);
            Assert.Equal(float.MaxValue, pmPrimitives.ReadFloat());
            
            pmPrimitives.WriteFloat(float.MinValue);
            Assert.Equal(float.MinValue, pmPrimitives.ReadFloat());
        }

        [Fact]
        public void OnWriteAndReadLong_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadLong_ShouldExecWithoutException));
            pmPrimitives.WriteLong(long.MaxValue);
            Assert.Equal(long.MaxValue, pmPrimitives.ReadLong());
            
            pmPrimitives.WriteLong(long.MinValue);
            Assert.Equal(long.MinValue, pmPrimitives.ReadLong());
        }

        [Fact]
        public void OnWriteAndReadUInt_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadUInt_ShouldExecWithoutException));
            pmPrimitives.WriteUInt(uint.MaxValue);
            Assert.Equal(uint.MaxValue, pmPrimitives.ReadUInt());
            
            pmPrimitives.WriteUInt(uint.MinValue);
            Assert.Equal(uint.MinValue, pmPrimitives.ReadUInt());
        }

        [Fact]
        public void OnWriteAndReadUShort_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadUShort_ShouldExecWithoutException));
            pmPrimitives.WriteUShort(ushort.MaxValue);
            Assert.Equal(ushort.MaxValue, pmPrimitives.ReadUShort());
            
            pmPrimitives.WriteUShort(ushort.MinValue);
            Assert.Equal(ushort.MinValue, pmPrimitives.ReadUShort());
        }

        [Fact]
        public void OnWriteAndReadShort_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadShort_ShouldExecWithoutException));
            pmPrimitives.WriteShort(short.MaxValue);
            Assert.Equal(short.MaxValue, pmPrimitives.ReadShort());
            
            pmPrimitives.WriteShort(short.MinValue);
            Assert.Equal(short.MinValue, pmPrimitives.ReadShort());
        }

        [Fact]
        public void OnWriteAndReadSByte_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadSByte_ShouldExecWithoutException));
            pmPrimitives.WriteSByte(sbyte.MaxValue);
            Assert.Equal(sbyte.MaxValue, pmPrimitives.ReadSByte());
            
            pmPrimitives.WriteSByte(sbyte.MinValue);
            Assert.Equal(sbyte.MinValue, pmPrimitives.ReadSByte());
        }
        
        [Fact]
        public void OnWriteAndReadByte_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadByte_ShouldExecWithoutException));
            pmPrimitives.WriteByte(byte.MaxValue);
            Assert.Equal(byte.MaxValue, pmPrimitives.ReadByte());
            
            pmPrimitives.WriteByte(byte.MinValue);
            Assert.Equal(byte.MinValue, pmPrimitives.ReadByte());
        }
        
        [Fact]
        public void OnWriteAndReadBool_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadBool_ShouldExecWithoutException));
            pmPrimitives.WriteBool(true);
            Assert.True(pmPrimitives.ReadBool());
            
            pmPrimitives.WriteBool(false);
            Assert.False(pmPrimitives.ReadBool());
        }

        [Fact]
        public void OnWriteAndReadInt_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadInt_ShouldExecWithoutException));
            pmPrimitives.WriteInt(int.MaxValue);
            Assert.Equal(int.MaxValue, pmPrimitives.ReadInt());

            pmPrimitives.WriteInt(int.MinValue);
            Assert.Equal(int.MinValue, pmPrimitives.ReadInt());
        }

        [Fact]
        public void OnWriteAndReadString_ShouldExecWithoutException()
        {
            var pmPrimitives = CreatePrimitive(nameof(OnWriteAndReadString_ShouldExecWithoutException));
            pmPrimitives.WriteString("Hello World!");
            Assert.Equal("Hello World!", pmPrimitives.ReadString());

            pmPrimitives.WriteString("Hello World!", offset: 20);
            Assert.Equal("Hello World!", pmPrimitives.ReadString(20));
        }

        private PmCSharpDefinedTypes CreatePrimitive(string methodName, long size = 4096)
        {
            return new PmCSharpDefinedTypes(CreatePmStream(methodName, size));
        }

        private static FileBasedStream CreatePmStream(string mappedMemoryFilePath, long size)
        {
            return new MemoryMappedStream(Path.Combine("D:\\temp\\pm_tests", mappedMemoryFilePath + ".pm"), size);
        }
    }
}
