﻿using System;
using PM.Tests.Common;
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
                CreatePmStream(
                    CreateFilePath(nameof(OnSetAndGet_ShouldRunWithoutException)),
                    length * sizeof(ulong)));

            array[0] = ulong.MaxValue;
            array[1] = ulong.MinValue;

            Assert.Equal(ulong.MaxValue, array[0]);
            Assert.Equal(ulong.MinValue, array[1]);
        }

        [Fact]
        public void OnSetAndGetOutOfBounds_ShouldThrowException()
        {
            var length = 1;
            var array = PmPrimitiveArray.CreateNewArray<ulong>(
                    new TraditionalMemoryMappedStream(CreateFilePath(nameof(OnSetAndGetOutOfBounds_ShouldThrowException)),
                    length * sizeof(ulong)));

            Assert.Throws<IndexOutOfRangeException>(() => array[1] = ulong.MaxValue);
            Assert.Throws<IndexOutOfRangeException>(() => array[1]);
            Assert.Throws<IndexOutOfRangeException>(() => array[-1]);
        }

        [Fact]
        public void OnHighVolume_ShouldNotThrowException()
        {
            var length = 500;
            var array = PmPrimitiveArray.CreateNewArray<ulong>(
                new TraditionalMemoryMappedStream(CreateFilePath(nameof(OnHighVolume_ShouldNotThrowException)),
                    length * sizeof(ulong)));

            for (int i = 0; i < length; i++)
            {
                array[i] = (ulong)length;
            }
        }
    }
}
