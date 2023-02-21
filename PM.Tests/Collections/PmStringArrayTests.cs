using PM.Collections;
using PM.Configs;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Tests.Collections
{
    public class PmStringArrayTests
    {
        public PmStringArrayTests()
        {
            if (Constraints.UseFakePm)
                PmGlobalConfiguration.PmTarget = PmTargets.InVolatileMemory;
        }

        [Fact]
        public void OnSetAndGet_ShouldRunWithoutException()
        {
            var array = new PmStringArray(nameof(OnSetAndGet_ShouldRunWithoutException), length: 2);

            var val0 = Guid.NewGuid().ToString();
            var val1 = Guid.NewGuid().ToString();
            array[0] = val0;
            array[1] = val1;

            Assert.Equal(val0, array[0]);
            Assert.Equal(val1, array[1]);
        }

        [Fact]
        public void OnSetAndGetStringOutOfBounds_ShouldThrowException()
        {
            var array = new PmStringArray(nameof(OnSetAndGetStringOutOfBounds_ShouldThrowException), length: 1);

            Assert.Throws<IndexOutOfRangeException>(() => array[1] = Guid.NewGuid().ToString());
            Assert.Throws<IndexOutOfRangeException>(() => array[1]);
        }
    }
}
