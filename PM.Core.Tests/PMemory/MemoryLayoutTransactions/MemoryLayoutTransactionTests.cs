using FileFormatExplain;
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
        public MemoryLayoutTransactionTests(ITestOutputHelper output)
            : base(output)
        {
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
            // 010d000000f5030000e9050000...
            // CommitByte=01
            // AddBlockOffset=0D
            // AddBlocksQtty=0D
            // RemoveBlockOffset=00
            // RemoveBlocksQtty=3F5
            // UpdateContentOffset=00
            // UpdateContentQtty=5E9
            Console.WriteLine(memoryLayoutTransactionDecoder.ExplainHex(buffer));
            Assert.StartsWith("010d000000f5030000e9050000", content);
        }


        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
