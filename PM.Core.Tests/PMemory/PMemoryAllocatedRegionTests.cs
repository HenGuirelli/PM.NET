using PM.Core.PMemory;
using PM.Tests.Common;
using System;
using System.Linq;
using Xunit;

namespace PM.Core.Tests.PMemory
{
    public class PMemoryAllocatedRegionTests : UnitTest
    {

        [Fact]
        public void OnAddPointerZero_ShouldAddPersistent()
        {
            DeleteFile(nameof(OnAddPointerZero_ShouldAddPersistent));

            var pmStream = CreatePmStream(nameof(OnAddPointerZero_ShouldAddPersistent), 4096);
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pmStream);
            var testingObject = new PMemoryAllocatedRegion(pmCSharpDefinedTypes);
            testingObject.AddPointer(0, sizeof(int));
            testingObject.AddPointer(1, sizeof(long));
            testingObject.AddPointer(2, sizeof(decimal));

            Assert.Equal(sizeof(int), testingObject.Pointers.ElementAt(0).Value);
            Assert.Equal(sizeof(long), testingObject.Pointers.ElementAt(1).Value);
            Assert.Equal(sizeof(decimal), testingObject.Pointers.ElementAt(2).Value);
        }

        [Fact]
        public void OnAddPointerZero_WhenTryAddZeroSizeAllocation_ShouldThrowException()
        {
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(
                CreatePmStream(nameof(OnAddPointerZero_WhenTryAddZeroSizeAllocation_ShouldThrowException), 4096));
            var testingObject = new PMemoryAllocatedRegion(pmCSharpDefinedTypes);
            Assert.Throws<ArgumentException>(() => testingObject.AddPointer(0, 0));
        }


        [Fact]
        public void OnAddPointerZero_WhenFileSizeIsInsufficient_ShouldThrowException()
        {
            var filename = nameof(OnAddPointerZero_WhenFileSizeIsInsufficient_ShouldThrowException);

            var pmStream = CreatePmStream(filename, size: 1); // Create a file with 1 bytes
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pmStream);
            var testingObject = new PMemoryAllocatedRegion(pmCSharpDefinedTypes);
            testingObject.AddPointer(0, sizeof(int)); // At this point, de file need to be resized.
            testingObject.AddPointer(1, sizeof(long));
            testingObject.AddPointer(2, sizeof(decimal));

            Assert.Equal(sizeof(int), testingObject.Pointers.ElementAt(0).Value);
            Assert.Equal(sizeof(long), testingObject.Pointers.ElementAt(1).Value);
            Assert.Equal(sizeof(decimal), testingObject.Pointers.ElementAt(2).Value);

            // Continue adding to increase file again.
            for(int i = 0; i < PMemoryAllocatedRegion.DefaultFileSize / sizeof(int); i++)
            {
                testingObject.AddPointer(i, sizeof(int));
            }
        }
    }
}
