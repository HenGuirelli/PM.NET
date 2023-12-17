using PM.Core.PMemory;
using PM.Tests.Common;
using System.IO;
using Xunit;

namespace PM.Core.Tests.PMemory
{
    public class PAllocatorTests : UnitTest
    {
        [Fact]
        public void OnCtor_ShouldCreatePMemoryLayout()
        {
            DeleteFile(nameof(OnCtor_ShouldCreatePMemoryLayout));

            var pmStream = CreatePmStream(nameof(OnCtor_ShouldCreatePMemoryLayout), 4096);

            var persistentAllocatorHeader = new PersistentAllocatorLayout();

            persistentAllocatorHeader.AddBlock(new PersistentBlockLayout(regionSize: 8, regionQuantity: 2));
            persistentAllocatorHeader.AddBlock(new PersistentBlockLayout(regionSize: 16, regionQuantity: 2));
            persistentAllocatorHeader.AddBlock(new PersistentBlockLayout(regionSize: 32, regionQuantity: 2));

            var pAllocator = new PAllocator(persistentAllocatorHeader, new PmCSharpDefinedTypes(pmStream));
            var filepath = pAllocator.FilePath;
            pAllocator.Dispose();

            string content = File.ReadAllText(filepath);
            // Assert commit byte equals 1 
            Assert.Equal(1, (byte)content[0]);
        }

        [Fact]
        public void OnRoundUpPow2_ShouldGetNextPowerOf2()
        {
            Assert.Equal(PAllocator.MinRegionSizeBytes, PAllocator.RoundUpPowerOfTwo(1));
            Assert.Equal(PAllocator.MinRegionSizeBytes, PAllocator.RoundUpPowerOfTwo(PAllocator.MinRegionSizeBytes));
            Assert.Equal(16, PAllocator.RoundUpPowerOfTwo(9));
            Assert.Equal(16, PAllocator.RoundUpPowerOfTwo(10));
            Assert.Equal(32, PAllocator.RoundUpPowerOfTwo(17));
        }

        [Fact]
        public void OnAllocate_ShouldAllocatePreDeterminateRegion()
        {
            DeleteFile(nameof(OnAllocate_ShouldAllocatePreDeterminateRegion));

            var pmStream = CreatePmStream(nameof(OnAllocate_ShouldAllocatePreDeterminateRegion), 4096);

            var persistentAllocatorLayout = new PersistentAllocatorLayout();

            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 8, regionQuantity: 2));
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 16, regionQuantity: 2));
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 32, regionQuantity: 2));

            var pAllocator = new PAllocator(persistentAllocatorLayout, new PmCSharpDefinedTypes(pmStream));

            
            var region = pAllocator.Alloc(1); // Should alloc 8 bytes region
            pAllocator.Alloc(8); // Should alloc 8 bytes region
            pAllocator.Alloc(9); // Should alloc 16 bytes region
            pAllocator.Alloc(16); // Should alloc 16 bytes region
        }
    }
}
