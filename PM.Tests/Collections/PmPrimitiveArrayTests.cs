using PM.Collections;
using PM.Configs;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Tests.Collections
{
    public class PmPrimitiveArrayTests
    {
        public PmPrimitiveArrayTests()
        {
            if (Constraints.UseFakePm)
                PmGlobalConfiguration.PmTarget = PmTargets.InVolatileMemory;
        }

        [Fact]
        public void OnSetAndGet_ShouldRunWithoutException()
        {
            var array = PmPrimitiveArray.CreateNewArray<ulong>(
                nameof(OnSetAndGet_ShouldRunWithoutException),
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
                nameof(OnSetAndGetOutOfBounds_ShouldThrowException),
                length: 1);

            Assert.Throws<IndexOutOfRangeException>(() => array[1] = ulong.MaxValue);
            Assert.Throws<IndexOutOfRangeException>(() => array[1]);
        }
    }
}
