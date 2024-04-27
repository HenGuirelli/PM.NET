using FileFormatExplain;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using PM.Core.PMemory.MemoryLayoutTransactions;
using PM.Tests.Common;
using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace PM.Core.Tests.PMemory.MemoryLayoutTransactions
{
    public class MemoryLayoutTransactionTests : UnitTest
    {
        private static readonly Random _random = new();
        private readonly ITestOutputHelper _output;

        public MemoryLayoutTransactionTests(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Fact]
        public void OnCctor_ShouldCreateLayout()
        {
            DeleteFile(nameof(OnCctor_ShouldCreateLayout));
            var pm = CreatePmStream(nameof(OnCctor_ShouldCreateLayout), 4096);
            var memoryLayoutTransaction = new MemoryLayoutTransaction(new PmCSharpDefinedTypes(pm));

            string content;
            var buffer = new byte[4096];
            using (FileStream fileStream = new(
                pm.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                fileStream.Read(buffer);
                content = ByteArrayToString(buffer);
            }

            var memoryLayoutTransactionDecoder = new MemoryLayoutTransactionDecoder();
            Console.WriteLine(memoryLayoutTransactionDecoder.ExplainHex(buffer));
            // 010d000000f5030000e9050000...
            // CommitByte=01
            // AddBlockOffset=0D
            // AddBlocksQtty=0D
            // RemoveBlockOffset=00
            // RemoveBlocksQtty=3F5
            // UpdateContentOffset=00
            // UpdateContentQtty=5E9
            Assert.StartsWith("010d000000f5030000e9050000", content);
        }

        [Fact]
        public void OnAdd_ShouldCreateAddBlocks()
        {
            DeleteFile(nameof(OnAdd_ShouldCreateAddBlocks));
            var pm = CreatePmStream(nameof(OnAdd_ShouldCreateAddBlocks), 4096);
            var memoryLayoutTransaction = new MemoryLayoutTransaction(new PmCSharpDefinedTypes(pm));

            memoryLayoutTransaction.AddBlock(new AddBlockLayout
            {
                BlockOffset = uint.MaxValue,
                RegionSize = 12,
                RegionsQtty = 13,
                Order = new Core.PMemory.FileFields.OrderField(AddBlockLayout.Offset.Order, instance: 1)
            });
            string content;
            byte[] buffer;
            ReadFile(pm.FilePath, out content, out buffer);

            var memoryLayoutTransactionDecoder = new MemoryLayoutTransactionDecoder();
            _output.WriteLine(memoryLayoutTransactionDecoder.ExplainHex(buffer));
            // 010d000100f5030000e9050000010100ffffffff0d0c
            // CommitByte=01
            // AddBlockOffset=000D
            // AddBlocksQtty=0001
            // RemoveBlockOffset=03F5
            // RemoveBlocksQtty=0000
            // UpdateContentOffset=05E9
            // UpdateContentQtty=0000
            // ===========Addblock===========
            // CommitByte=01
            // Order=0001
            // StartBlockOffset=FFFFFFFF
            // RegionsQtty=0D
            // RegionsSize=000C
            Assert.StartsWith("010d000100f5030000e9050000010100ffffffff0d0c", content);

            memoryLayoutTransaction.AddBlock(new AddBlockLayout
            {
                BlockOffset = uint.MinValue,
                RegionSize = uint.MaxValue,
                RegionsQtty = byte.MaxValue,
                Order = new Core.PMemory.FileFields.OrderField(AddBlockLayout.Offset.Order, instance: 1)
            });

            ReadFile(pm.FilePath, out content, out buffer);

            // 010d000200f5030000e9050000010100ffffffff0d0c00000001020000000000ffffffffff00000000
            // CommitByte=01
            // AddBlockOffset=000D
            // AddBlocksQtty=0002
            // RemoveBlockOffset=03F5
            // RemoveBlocksQtty=0000
            // UpdateContentOffset=05E9
            // UpdateContentQtty=0000
            // ===========Addblock===========
            // CommitByte=01
            // Order=0001
            // StartBlockOffset=FFFFFFFF
            // RegionsQtty=0D
            // RegionsSize=000C
            // ===========Addblock===========
            // CommitByte=01
            // Order=0002
            // StartBlockOffset=00000000
            // RegionsQtty=FF
            // RegionsSize=FFFF
            _output.WriteLine(memoryLayoutTransactionDecoder.ExplainHex(buffer));
            Assert.StartsWith("010d000200f5030000e9050000010100ffffffff0d0c00000001020000000000ffffffffff00000000", content);
        }

        [Fact]
        public void OnAdd_WhenHaveOneInvalidBlock_ShouldRecycleRegion()
        {
            DeleteFile(nameof(OnAdd_WhenHaveOneInvalidBlock_ShouldRecycleRegion));
            var transactionFilePm = CreatePmStream(nameof(OnAdd_WhenHaveOneInvalidBlock_ShouldRecycleRegion), 4096);
            var originalFilePm = CreatePmStream("simulateOriginal", 4096);
            var memoryLayoutTransaction = new MemoryLayoutTransaction(new PmCSharpDefinedTypes(transactionFilePm), new Core.PMemory.PersistentAllocatorLayout
            {
                PmCSharpDefinedTypes = new PmCSharpDefinedTypes(originalFilePm)
            });

            // Insert two blocks layouts
            memoryLayoutTransaction.AddBlock(new AddBlockLayout
            {
                BlockOffset = uint.MaxValue,
                RegionSize = 16,
                RegionsQtty = 13,
                Order = new Core.PMemory.FileFields.OrderField(AddBlockLayout.Offset.Order, instance: 2)
            });
            memoryLayoutTransaction.AddBlock(new AddBlockLayout
            {
                BlockOffset = uint.MinValue,
                RegionSize = 8,
                RegionsQtty = 64,
                Order = new Core.PMemory.FileFields.OrderField(AddBlockLayout.Offset.Order, instance: 2)
            });

            memoryLayoutTransaction.CommitBlockLayouts(qtty: 1);


            string content;
            byte[] buffer;
            ReadFile(transactionFilePm.FilePath, out content, out buffer);

            // 010d000200f5030000e9050000020100ffffffff0d1000000001020000000000400800000000
            // CommitByte=01
            // AddBlockOffset=000D
            // AddBlocksQtty=0002
            // RemoveBlockOffset=03F5
            // RemoveBlocksQtty=0000
            // UpdateContentOffset=05E9
            // UpdateContentQtty=0000
            // ===========Addblock===========
            // CommitByte=02
            // Order=0001
            // StartBlockOffset=FFFFFFFF
            // RegionsQtty=0D
            // RegionsSize=0010
            // ===========Addblock===========
            // CommitByte=01
            // Order=0002
            // StartBlockOffset=00000000
            // RegionsQtty=40
            // RegionsSize=0008
            var memoryLayoutTransactionDecoder = new MemoryLayoutTransactionDecoder();
            _output.WriteLine(memoryLayoutTransactionDecoder.ExplainHex(buffer));
            Assert.StartsWith("010d000200f5030000e9050000020100ffffffff0d1000000001020000000000400800000000", content);
        }

        private static void ReadFile(string filePath, out string content, out byte[] buffer)
        {
            buffer = new byte[4096];
            using (FileStream fileStream = new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                fileStream.Read(buffer);
                content = ByteArrayToString(buffer);
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
