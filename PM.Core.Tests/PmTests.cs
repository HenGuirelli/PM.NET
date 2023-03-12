using PM.Configs;
using PM.Core.Fakes;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Core.Tests
{
    public class PmTests : UnitTest
    {
        private static readonly Random _random = new();

        [Fact]
        public void OnLoadStoreByte_ShouldExecWithoutException()
        {
            var pm = CreatePm(nameof(OnLoadStoreByte_ShouldExecWithoutException));

            Assert.True(pm.Store(byte.MaxValue));
            var value1 = pm.Load();
            Assert.Equal(byte.MaxValue, value1);

            // Test with offset
            Assert.True(pm.Store(byte.MinValue, offset: sizeof(byte)));
            var value2 = pm.Load(offset: sizeof(byte));
            Assert.Equal(byte.MinValue, value2);
        }

        [Fact]
        public void OnLoadStoreByteArray_ShouldExecWithoutException()
        {
            var pm = CreatePm(nameof(OnLoadStoreByteArray_ShouldExecWithoutException));

            var randomArray = new byte[_random.Next(pm.PmMemoryMappedFileConfig.SizeBytes)];
            _random.NextBytes(randomArray);

            Assert.True(pm.Store(randomArray));
            var byteArrayReadFromPm = pm.Load(byteCount: randomArray.Length);
            for (int i = 0; i < randomArray.Length; i++)
            {
                Assert.Equal(randomArray[i], byteArrayReadFromPm[i]);
            }
        } 

        [Fact]
        public void OnLoadStoreByteArray_WithOffset_ShouldExecWithoutException()
        {
            var pm = CreatePm(nameof(OnLoadStoreByteArray_WithOffset_ShouldExecWithoutException));

            var randomArray = new byte[_random.Next(pm.PmMemoryMappedFileConfig.SizeBytes / 2)];
            _random.NextBytes(randomArray);

            Assert.True(pm.Store(randomArray, offset: pm.PmMemoryMappedFileConfig.SizeBytes / 2));
            var byteArrayReadFromPm = pm.Load(byteCount: randomArray.Length, offset: pm.PmMemoryMappedFileConfig.SizeBytes / 2);
            for (int i = 0; i < randomArray.Length; i++)
            {
                Assert.Equal(randomArray[i], byteArrayReadFromPm[i]);
            }
        }
    }
}