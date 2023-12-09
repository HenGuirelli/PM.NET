using PM.Core.PMemory;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Core.Tests.PMemory
{
    public class PersistentBlockLayoutTests : UnitTest
    {
        [Fact]
        public void OnCtor_ShouldValidateRegionSizePowerOfTwo()
        {
            Assert.Throws<ArgumentException>(() => new PersistentBlockLayout(regionSize: 7, regionQuantity: 1));
            Assert.Throws<ArgumentException>(() => new PersistentBlockLayout(regionSize: 0, regionQuantity: 1));
            Assert.Throws<ArgumentException>(() => new PersistentBlockLayout(regionSize: -1, regionQuantity: 1));

            Exception? exception = null;
            exception = Record.Exception(() => new PersistentBlockLayout(regionSize: (int)Math.Pow(2, 1), regionQuantity: 1));
            Assert.Null(exception);
            exception = Record.Exception(() => new PersistentBlockLayout(regionSize: (int)Math.Pow(2, 2), regionQuantity: 1));
            Assert.Null(exception);
            exception = Record.Exception(() => new PersistentBlockLayout(regionSize: (int)Math.Pow(2, 3), regionQuantity: 1));
            Assert.Null(exception);
            exception = Record.Exception(() => new PersistentBlockLayout(regionSize: (int)Math.Pow(2, 4), regionQuantity: 1));
            Assert.Null(exception);
            exception = Record.Exception(() => new PersistentBlockLayout(regionSize: (int)Math.Pow(2, 5), regionQuantity: 1));
            Assert.Null(exception);
        }

        [Fact]
        public void OnCtor_ShouldValidateRegionQuantityGreaterThanZero()
        {
            Assert.Throws<ArgumentException>(() => new PersistentBlockLayout(regionSize: (int)Math.Pow(2, 1), regionQuantity: 0));

            var exception = Record.Exception(() => new PersistentBlockLayout(regionSize: (int)Math.Pow(2, 1), regionQuantity: 1));
            Assert.Null(exception);
        }
    }
}
