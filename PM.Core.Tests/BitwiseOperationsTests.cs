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
            Assert.Equal(1, BitwiseOperations.RoundUpPowerOfTwo(1));
            Assert.Equal(16, BitwiseOperations.RoundUpPowerOfTwo(9));
            Assert.Equal(16, BitwiseOperations.RoundUpPowerOfTwo(10));
            Assert.Equal(32, BitwiseOperations.RoundUpPowerOfTwo(17));
        }

        [Theory]
        [InlineData(
            0b00000000,
            new int[] { },
            new int[] { 0, 1, 2, 3, 4, 5, 6, 7 })]
        [InlineData(
            0b00001111,
            new int[] { 4, 5, 6, 7 },
            new int[] { 0, 1, 2, 3 })]
        [InlineData(
            0b11110000,
            new int[] { 0, 1, 2, 3 },
            new int[] { 4, 5, 6, 7 })]
        [InlineData(
            0b10101010,
            new int[] { 0, 2, 4, 6 },
            new int[] { 1, 3, 5, 7 })]
        public void OnVerifyBit_LittleEndian(ulong bitmap, int[] indexBitOn, int[] indexBitOff)
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
