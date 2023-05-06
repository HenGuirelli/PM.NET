using PM.PmContent;
using PM.Startup;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Tests.Startup
{
    public class ClassHashManagerTest : UnitTest
    {
        private static readonly Random _random = new();

        [Fact]
        public void OnAddHashFile_ShouldCreateAndWriteFile()
        {
            var classHashManager = new ClassHashManager();
            var obj = new ComplexClassWithSelfReference();
            ulong randomPointer = (ulong)_random.Next();

            classHashManager.AddHashFile(obj.GetType(), randomPointer);

            var hashFile = classHashManager.GetHashFile(obj.GetType());

            Assert.Equal(ClassHashCodeCalculator.GetHashCode(obj.GetType()), hashFile!.Hash);
            Assert.Equal(randomPointer, hashFile!.Pointer);
        }
    }
}
