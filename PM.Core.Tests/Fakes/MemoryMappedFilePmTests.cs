﻿using PM.Core.Fakes;
using Xunit;

namespace PM.Core.Tests.Fakes
{
    [Collection("PM.UnitTests")]
    public class MemoryMappedFilePmTests
    {
        [Fact]
        public void OnStoreAndLoadByte_ShouldWriteAndLoadCorrectly()
        {
            var testingObject = new MemoryMappedFilePm(new PmMemoryMappedFileConfig(nameof(OnStoreAndLoadByte_ShouldWriteAndLoadCorrectly)));
            testingObject.Store(byte.MaxValue);
            testingObject.Store(byte.MinValue, offset: 4);

            Assert.Equal(byte.MaxValue, testingObject.Load());
            Assert.Equal(byte.MinValue, testingObject.Load(offset: 4));
        }
    }
}
