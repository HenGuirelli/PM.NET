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
                RegionsQtty = 13
            });

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
            _output.WriteLine(memoryLayoutTransactionDecoder.ExplainHex(buffer));
            // 010d000100f5030000e9050000010000ffffffff0d0c0000000000000...
            // CommitByte=01
            // AddBlockOffset=0D
            // AddBlocksQtty=01
            // RemoveBlockOffset=3F5
            // RemoveBlocksQtty=00
            // UpdateContentOffset=5E9
            // UpdateContentQtty=00
            // ===========Addblock===========
            // CommitByte=01
            // Order=00
            // StartBlockOffset=FFFFFFFF
            // RegionsQtty=0D
            // RegionsSize=0C
            Assert.StartsWith("010d000100f5030000e9050000010000ffffffff0d0c", content);
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
