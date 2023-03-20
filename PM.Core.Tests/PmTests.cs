using System;
using System.IO;
using Xunit;

namespace PM.Core.Tests
{
    public class PmTests
    {
        private static readonly Random _random = new();

        [Fact]
        public void OnLoadStoreByte_ShouldExecWithoutException()
        {
            var pm = CreatePmStream(nameof(OnLoadStoreByte_ShouldExecWithoutException));
            pm.WriteByte(byte.MaxValue);
            pm.Seek(0, SeekOrigin.Begin);
            var value1 = pm.ReadByte();
            Assert.Equal(byte.MaxValue, value1);

            // Test with offset
            pm.Seek(sizeof(byte), SeekOrigin.Begin);
            pm.WriteByte(byte.MinValue);

            pm.Seek(sizeof(byte), SeekOrigin.Begin);
            Assert.Equal(byte.MinValue, pm.ReadByte());
        }

        [Fact]
        public void OnLoadStoreByteArray_ShouldExecWithoutException()
        {
            var pm = CreatePmStream(nameof(OnLoadStoreByteArray_ShouldExecWithoutException));

            var randomArray = new byte[_random.Next((int)pm.Length)];
            _random.NextBytes(randomArray);

            pm.Write(randomArray);
            var byteArrayReadFromPm = new byte[randomArray.Length];
            pm.Seek(0, SeekOrigin.Begin);
            pm.Read(byteArrayReadFromPm);
            for (int i = 0; i < randomArray.Length; i++)
            {
                Assert.Equal(randomArray[i], byteArrayReadFromPm[i]);
            }
        } 

        [Fact]
        public void OnLoadStoreByteArray_WithOffset_ShouldExecWithoutException()
        {
            var pm = CreatePmStream(nameof(OnLoadStoreByteArray_WithOffset_ShouldExecWithoutException));

            var randomArray = new byte[_random.Next((int)pm.Length / 2)];
            _random.NextBytes(randomArray);

            pm.Seek(pm.Length / 2, SeekOrigin.Begin);
            pm.Write(randomArray);
            pm.Seek(pm.Length / 2, SeekOrigin.Begin);
            var byteArrayReadFromPm = new byte[randomArray.Length];
            pm.Read(byteArrayReadFromPm);
            for (int i = 0; i < randomArray.Length; i++)
            {
                Assert.Equal(randomArray[i], byteArrayReadFromPm[i]);
            }
        }

        private static Stream CreatePmStream(string mappedMemoryFilePath, long size=4096)
        {
            return new MemoryMappedStream(Path.Combine("D:\\temp\\pm_tests", mappedMemoryFilePath + ".pm"), size);
        }
    }
}