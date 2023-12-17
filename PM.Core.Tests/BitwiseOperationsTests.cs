using PM.Core.PMemory;
using System;
using System.Buffers.Binary;
using Xunit;

namespace PM.Core.Tests
{
    public class BitwiseOperationsTests
    {
        [Fact]
        public void OnRoundUpPowerOfTwo_ShouldGetNextPowerOfTwo()
        {
            Assert.Equal(PAllocator.MinRegionSizeBytes, BitwiseOperations.RoundUpPowerOfTwo(PAllocator.MinRegionSizeBytes));
            Assert.Equal(1, BitwiseOperations.RoundUpPowerOfTwo(1));
            Assert.Equal(16, BitwiseOperations.RoundUpPowerOfTwo(9));
            Assert.Equal(16, BitwiseOperations.RoundUpPowerOfTwo(10));
            Assert.Equal(32, BitwiseOperations.RoundUpPowerOfTwo(17));
        }

        [Theory]
        [InlineData(
            new byte[] { 0b00000000 },
            new int[] { },
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [InlineData(
            new byte[] { 0b00001111 },
            new int[] { 4, 5, 6, 7 },
            new int[] { 0, 1, 2, 3 })]
        [InlineData(
            new byte[] { 0b11110000 },
            new int[] { 0, 1, 2, 3 },
            new int[] { 4, 5, 6, 7 })]
        [InlineData(
            new byte[] { 0b10101010 },
            new int[] { 0, 2, 4, 6 },
            new int[] { 1, 3, 5, 7 })]
        [InlineData(
            new byte[] { 0b11001100, 0b00110011 },
            new int[] { 0, 1, 10, 15 },
            new int[] { 2, 8, 9, 12, 13 })]
        public void OnVerifyBit_LittleEndian(byte[] bitmap, int[] indexBitOn, int[] indexBitOff)
        {
            foreach (var bitIndex in indexBitOn)
            {
                Assert.True(BitwiseOperations.VerifyBit(bitmap, bitIndex));
            }

            foreach (var bitIndex in indexBitOff)
            {
                Assert.False(BitwiseOperations.VerifyBit(bitmap, bitIndex));
            }
        }
    }
}
