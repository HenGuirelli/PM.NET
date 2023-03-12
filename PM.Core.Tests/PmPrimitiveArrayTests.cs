using PM.Factories;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Core.Tests
{
    public class PmPrimitiveArrayTests : UnitTest
    {
        [Fact]
        public void OnSetAndGet_ShouldRunWithoutException()
        {
            var length = 2;
            var array = PmPrimitiveArray.CreateNewArray<ulong>(
                PmFactory.CreatePm(new PmMemoryMappedFileConfig(
                    CreateFilePath(nameof(OnSetAndGet_ShouldRunWithoutException)),
                    size: length * sizeof(ulong))),
                    length: length);

            array[0] = ulong.MaxValue;
            array[1] = ulong.MinValue;

            Assert.Equal(ulong.MaxValue, array[0]);
            Assert.Equal(ulong.MinValue, array[1]);
        }

        [Fact]
        public void OnSetAndGetOutOfBounds_ShouldThrowException()
        {
            var array = PmPrimitiveArray.CreateNewArray<ulong>(
                PmFactory.CreatePm(new PmMemoryMappedFileConfig(
                     CreateFilePath(nameof(OnSetAndGetOutOfBounds_ShouldThrowException)))),
                    length: 1);

            Assert.Throws<IndexOutOfRangeException>(() => array[1] = ulong.MaxValue);
            Assert.Throws<IndexOutOfRangeException>(() => array[1]);
            Assert.Throws<IndexOutOfRangeException>(() => array[-1]);
        }

        [Fact]
        public void OnHighVolume_ShouldNotThrowException()
        {
            var count = 500;
            var array = PmPrimitiveArray.CreateNewArray<ulong>(
                PmFactory.CreatePm(new PmMemoryMappedFileConfig(
                     CreateFilePath(nameof(OnHighVolume_ShouldNotThrowException)))),
                    length: count);

            for (int i = 0; i < count; i++)
            {
                array[i] = (ulong)count;
            }
        }
    }
}
