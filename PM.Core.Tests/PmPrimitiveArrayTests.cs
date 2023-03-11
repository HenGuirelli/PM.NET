using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Core.Tests
{
    public class PmPrimitiveArrayTests
    {
        private const string PrefixPath = "D:\\temp\\pm_tests\\";

        public PmPrimitiveArrayTests()
        {
            PmGlobalConfiguration.PmTarget = Constraints.PmTarget;
        }

        [Fact]
        public void OnSetAndGet_ShouldRunWithoutException()
        {
            var array = PmPrimitiveArray.CreateNewArray<ulong>(
                PmFactory.CreatePm(new PmMemoryMappedFileConfig(
                    PrefixPath + nameof(OnSetAndGet_ShouldRunWithoutException))),
                    length: 2);

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
                     PrefixPath + nameof(OnSetAndGetOutOfBounds_ShouldThrowException))),
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
                     PrefixPath + nameof(OnHighVolume_ShouldNotThrowException))),
                    length: count);

            for (int i = 0; i < count; i++)
            {
                array[i] = (ulong)count;
            }
        }
    }
}
