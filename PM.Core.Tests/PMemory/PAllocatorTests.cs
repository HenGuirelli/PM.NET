using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using PM.Core.PMemory;
using PM.Tests.Common;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;
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

            // Write/Read first region with 8 bytes
            var region8Bytes = pAllocator.Alloc(sizeof(int) * 2);
            region8Bytes.Write(BitConverter.GetBytes(int.MaxValue), offset: 0);
            region8Bytes.Write(BitConverter.GetBytes(int.MinValue), offset: sizeof(int));

            var number1 = BitConverter.ToInt32(region8Bytes.GetData(sizeof(int), offset: 0));
            var number2 = BitConverter.ToInt32(region8Bytes.GetData(sizeof(int), offset: sizeof(int)));

            Assert.Equal(int.MaxValue, number1);
            Assert.Equal(int.MinValue, number2);


            // Write/Read second region with 16 bytes
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
        public void OnWriteRegion_WhenDontHaveAnyBlock_ShouldCreateBlocks()
        {
            DeleteFile(nameof(OnWriteRegion_WhenDontHaveAnyBlock_ShouldCreateBlocks));

            var pmStream = CreatePmStream(nameof(OnWriteRegion_WhenDontHaveAnyBlock_ShouldCreateBlocks), 4096 * 2);

            var persistentAllocatorLayout = new PersistentAllocatorLayout();
            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);

            // Create new block and region
            var region = pAllocator.Alloc(1);
            region.Write(new byte[] { byte.MaxValue }, offset: 0);

            var newRegion = pAllocator.Alloc(4096);
            newRegion.Write(BitConverter.GetBytes(long.MaxValue), offset: 0);
        }

        [Fact]
        public void OnLoad_WhenDontHaveAnyFile_ShouldCreateEmptyLayout()
        {
            DeleteFile(nameof(OnLoad_WhenDontHaveAnyFile_ShouldCreateEmptyLayout));
            var pmStream = CreatePmStream(nameof(OnLoad_WhenDontHaveAnyFile_ShouldCreateEmptyLayout), 4096 * 2);
            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            Assert.False(pAllocator.IsLayoutCreated());
            // Should create a empty layout
            pAllocator.Load();
            Assert.True(pAllocator.IsLayoutCreated());
        }

        [Fact]
        public void OnLoad_WhenHaveOneBlock_ShouldLoadPMemory()
        {
            DeleteFile(nameof(OnLoad_WhenHaveOneBlock_ShouldLoadPMemory));

            // Create pmemory file
            var pmStream = CreatePmStream(nameof(OnLoad_WhenHaveOneBlock_ShouldLoadPMemory), 4096 * 2);
            var persistentAllocatorLayout = new PersistentAllocatorLayout();
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 16, regionQuantity: 10));
            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);
            var region = pAllocator.Alloc(16);
            // Write some data to recovery later
            region.Write(BitConverter.GetBytes(long.MaxValue), offset: 0);

            // Unload allocator and release all file handlers
            pAllocator.Dispose();

            // Create new PAllocator object.
            // This object should load the previous
            // layout.
            pmStream = CreatePmStream(nameof(OnLoad_WhenHaveOneBlock_ShouldLoadPMemory), 4096 * 2);
            pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            Assert.True(pAllocator.IsLayoutCreated());
            // Load previous layout
            pAllocator.Load();

            // Get the previous region
            region = pAllocator.GetRegion(1, region.RegionIndex);
            Assert.Equal(long.MaxValue, BitConverter.ToInt64(region.GetData(8, offset: 0)));
        } 
        
        [Fact]
        public void OnLoad_WhenBlocksAreCreatedDynamically_ShouldLoadPMemory()
        {
            DeleteFile(nameof(OnLoad_WhenBlocksAreCreatedDynamically_ShouldLoadPMemory));

            // Create pmemory file
            var pmStream = CreatePmStream(nameof(OnLoad_WhenBlocksAreCreatedDynamically_ShouldLoadPMemory), 4096 * 2);
            var persistentAllocatorLayout = new PersistentAllocatorLayout();
            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);
            // Alloc a region dinamically
            var region = pAllocator.Alloc(16);
            // Write some data to recovery later
            region.Write(BitConverter.GetBytes(long.MaxValue), offset: 0);

            // Unload allocator and release all file handlers
            pAllocator.Dispose();

            // Create new PAllocator object.
            // This object should load the previous
            // layout.
            pmStream = CreatePmStream(nameof(OnLoad_WhenBlocksAreCreatedDynamically_ShouldLoadPMemory), 4096 * 2);
            pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            Assert.True(pAllocator.IsLayoutCreated());
            // Load previous layout
            pAllocator.Load();

            // Get the previous region
            region = pAllocator.GetRegion(1, region.RegionIndex);
            Assert.Equal(long.MaxValue, BitConverter.ToInt64(region.GetData(8, offset: 0)));
        } 
        
        [Fact]
        public void OnLoad_WhenHaveMultipleBlocks_ShouldLoadPMemory()
        {
            DeleteFile(nameof(OnLoad_WhenHaveMultipleBlocks_ShouldLoadPMemory));

            // Create pmemory file
            var pmStream = CreatePmStream(nameof(OnLoad_WhenHaveMultipleBlocks_ShouldLoadPMemory), 4096 * 2);
            var persistentAllocatorLayout = new PersistentAllocatorLayout();
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 16, regionQuantity: 10));
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 32, regionQuantity: 1));
            persistentAllocatorLayout.AddBlock(new PersistentBlockLayout(regionSize: 64, regionQuantity: 64));
            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);
            // Write some data in block 1 to recovery later
            var region1 = pAllocator.Alloc(16);
            region1.Write(BitConverter.GetBytes(long.MaxValue), offset: 0);
            // Write some data in block 2 to recovery later
            var region2 = pAllocator.Alloc(32);
            region2.Write(BitConverter.GetBytes(long.MinValue), offset: 1);
            // Write some data in block 3 to recovery later
            var region3 = pAllocator.Alloc(64);
            region3.Write(BitConverter.GetBytes(long.MinValue / 2), offset: 2);

            // Unload allocator and release all file handlers
            pAllocator.Dispose();

            // Create new PAllocator object.
            // This object should load the previous
            // layout.
            pmStream = CreatePmStream(nameof(OnLoad_WhenHaveMultipleBlocks_ShouldLoadPMemory), 4096 * 2);
            pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            Assert.True(pAllocator.IsLayoutCreated());
            // Load previous layout
            pAllocator.Load();

            var blockIds = pAllocator.PersistentAllocatorLayout?.Blocks.Select(x => x.BlockOffset).ToList();

            // Get the previous region
            region1 = pAllocator.GetRegion(blockIds[0], region1.RegionIndex);
            Assert.Equal(long.MaxValue, BitConverter.ToInt64(region1.GetData(8, offset: 0)));
            region2 = pAllocator.GetRegion(blockIds[1], region2.RegionIndex);
            Assert.Equal(long.MinValue, BitConverter.ToInt64(region2.GetData(8, offset: 1)));
            region3 = pAllocator.GetRegion(blockIds[2], region3.RegionIndex);
            Assert.Equal(long.MinValue / 2, BitConverter.ToInt64(region3.GetData(8, offset: 2)));
        }

        private static readonly Random _random = new();
        [Fact]
        public void RandomTest()
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Fatal) // Disable log
            .WriteTo.Console()
            .CreateLogger();

            DeleteFile(nameof(RandomTest));
            var pmStream = CreatePmStream(nameof(RandomTest), 4096);
            var persistentAllocatorLayout = new PersistentAllocatorLayout();
            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);

            var allocQty = _random.Next(1000, 100_000);
            for (var i = 0; i < allocQty; i++)
            {
                var regionSize = _random.Next(100_000);
                var region = pAllocator.Alloc(regionSize);
                var value = new byte[] { 1, 1, 1 };
                region.Write(value, _random.Next(regionSize - value.Length));
            }
        } 
        
        [Fact]
        public void OnAlloc_AllocSameSizeALotOfTimes()
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Fatal) // Disable log
            .WriteTo.Console()
            .CreateLogger();

            DeleteFile(nameof(OnAlloc_AllocSameSizeALotOfTimes));
            var pmStream = CreatePmStream(nameof(OnAlloc_AllocSameSizeALotOfTimes), 4096);
            var persistentAllocatorLayout = new PersistentAllocatorLayout();
            var pAllocator = new PAllocator(new PmCSharpDefinedTypes(pmStream));
            pAllocator.CreateLayout(persistentAllocatorLayout);

            var allocQty = _random.Next(1000, 100_000);
            for (var i = 0; i < allocQty; i++)
            {
                var regionSize = 4096;
                var region = pAllocator.Alloc(regionSize);
                var value = new byte[] { 1, 1, 1 };
                region.Write(value, _random.Next(regionSize - value.Length));
            }
        }
    }
}
