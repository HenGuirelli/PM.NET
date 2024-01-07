using PM.Core.PMemory;
using PM.Tests.Common;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace PM.Core.Tests.PMemory
{
    public class PAllocatorTests : UnitTest
    {
        public PAllocatorTests(ITestOutputHelper output) 
            : base(output)
        {
        }

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

        [Fact]
        public void OnWriteAndReadRegion_ShouldWriteAndReadWithoutException()
        {
            DeleteFile(nameof(OnWriteAndReadRegion_ShouldWriteAndReadWithoutException));

            var pmStream = CreatePmStream(nameof(OnWriteAndReadRegion_ShouldWriteAndReadWithoutException), 4096);

            var persistentAllocatorLayout = new PersistentAllocatorLayout();

            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 8, regionQuantity: 2));
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 16, regionQuantity: 2));

            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);

            // Write/Read First region 8 bytes
            var region8Bytes = pAllocator.Alloc(sizeof(int) * 2);
            region8Bytes.Write(BitConverter.GetBytes(int.MaxValue), offset: 0);
            region8Bytes.Write(BitConverter.GetBytes(int.MinValue), offset: sizeof(int));

            var number1 = BitConverter.ToInt32(region8Bytes.GetData(sizeof(int), offset: 0));
            var number2 = BitConverter.ToInt32(region8Bytes.GetData(sizeof(int), offset: sizeof(int)));

            Assert.Equal(int.MaxValue, number1);
            Assert.Equal(int.MinValue, number2);


            // Write/Read First region 16 bytes
            var region16Bytes = pAllocator.Alloc(sizeof(long) * 2);
            region16Bytes.Write(BitConverter.GetBytes(long.MaxValue), offset: 0);
            region16Bytes.Write(BitConverter.GetBytes(long.MinValue), offset: sizeof(long));

            var number1_16bytes = BitConverter.ToInt64(region16Bytes.GetData(sizeof(long), offset: 0));
            var number2_16bytes = BitConverter.ToInt64(region16Bytes.GetData(sizeof(long), offset: sizeof(long)));

            Assert.Equal(long.MaxValue, number1_16bytes);
            Assert.Equal(long.MinValue, number2_16bytes);
        } 
        
        [Fact]
        public void OnWriteRegion_ShouldThrowAccessViolationExcpetion()
        {
            DeleteFile(nameof(OnWriteRegion_ShouldThrowAccessViolationExcpetion));

            var pmStream = CreatePmStream(nameof(OnWriteRegion_ShouldThrowAccessViolationExcpetion), 4096);

            var persistentAllocatorLayout = new PersistentAllocatorLayout();

            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 4, regionQuantity: 2));
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 8, regionQuantity: 2));

            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream)) { MinRegionSizeBytes = 2 };
            pAllocator.CreateLayout(persistentAllocatorLayout);

            var region4Bytes = pAllocator.Alloc(4);
            // Try write 8 bytes in 4 bytes region
            Assert.Throws<AccessViolationException>(() => region4Bytes.Write(BitConverter.GetBytes(long.MaxValue), offset: 0));

            var region8bytes = pAllocator.Alloc(8);
            Assert.Throws<AccessViolationException>(() => region8bytes.Write(new byte[] { 0 }, offset: 8));
        }

        [Fact]
        public void OnWriteRegion_WhenDontHaveAnyRegion_ShouldCreateRegion()
        {
            DeleteFile(nameof(OnWriteRegion_WhenDontHaveAnyRegion_ShouldCreateRegion));

            var pmStream = CreatePmStream(nameof(OnWriteRegion_WhenDontHaveAnyRegion_ShouldCreateRegion), 4096 * 2);

            var persistentAllocatorLayout = new PersistentAllocatorLayout();
            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);
            
            // Create new block and region
            var region = pAllocator.Alloc(1);
            region.Write(new byte[] { byte.MaxValue }, offset: 0);

            var newRegion = pAllocator.Alloc(4096);
            newRegion.Write(BitConverter.GetBytes(long.MaxValue), offset: 0);
        }
    }
}
