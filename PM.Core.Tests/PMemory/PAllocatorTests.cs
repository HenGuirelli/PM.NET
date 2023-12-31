using PM.Core.PMemory;
using PM.Tests.Common;
using System.IO;
using Xunit;

namespace PM.Core.Tests.PMemory
{
    public class PAllocatorTests : UnitTest
    {
        [Fact]
        public void OnCreateLayout_ShouldCreatePMemoryLayout()
        {
            DeleteFile(nameof(OnCreateLayout_ShouldCreatePMemoryLayout));

            var pmStream = CreatePmStream(nameof(OnCreateLayout_ShouldCreatePMemoryLayout), 4096);

            var persistentAllocatorLayout = new PersistentAllocatorLayout();

            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 8, regionQuantity: 2));
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 16, regionQuantity: 2));
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 32, regionQuantity: 2));

            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);

            var filepath = pAllocator.FilePath;
            pAllocator.Dispose();
            string content = File.ReadAllText(filepath);
            // Assert commit byte equals 1 
            Assert.Equal(1, (byte)content[0]);
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

            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);

            var region = pAllocator.Alloc(1); // Should alloc 8 bytes region
            Assert.Equal(8, region.Size);
            
            region = pAllocator.Alloc(8); // Should alloc 8 bytes region
            Assert.Equal(8, region.Size);
            
            region = pAllocator.Alloc(9); // Should alloc 16 bytes region
            Assert.Equal(16, region.Size);
            
            region = pAllocator.Alloc(16); // Should alloc 16 bytes region
            Assert.Equal(16, region.Size);
        }
    }
}
