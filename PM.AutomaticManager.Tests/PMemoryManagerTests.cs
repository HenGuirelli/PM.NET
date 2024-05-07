using FileFormatExplain;
using PM.Common;
using PM.Tests.Common;
using System.Buffers;
using Xunit.Abstractions;

namespace PM.AutomaticManager.Tests
{
    public class TestClass
    {
        public int Int { get; set; }
    }

    public class PMemoryManagerTests : UnitTest
    {
        private const string OriginalFileSufix = "_OriginalFile.pm";
        private const string TransactionFileSufix = "_TransactionFile.pm";

        public PMemoryManagerTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void Test1()
        {
            DeleteFilesByMethodName(nameof(Test1));
            var streams = CreateStreamByMethodName(nameof(Test1));

            var pAllocator = new FileEngine.PAllocator(streams.OriginalStream, streams.TransactionStream);
            var pMemoryManager = new PMemoryManager(pAllocator);

            var testobj = new TestClass
            {
                Int = int.MaxValue
            };
            pMemoryManager.AddNewObject("id object to restore later", testobj);

            var result = PMemoryDecoder.DecodeHex(pAllocator.ReadOriginalFile());
        }


        private static PmStreams CreateStreamByMethodName(string methodName)
        {
            return new PmStreams
            {
                OriginalStream = new PmCSharpDefinedTypes(CreatePmStream(methodName + OriginalFileSufix)),
                TransactionStream = new PmCSharpDefinedTypes(CreatePmStream(methodName + TransactionFileSufix))
            };
        }

        private static void DeleteFilesByMethodName(string methodName)
        {
            DeleteFile(methodName + OriginalFileSufix);
            DeleteFile(methodName + TransactionFileSufix);
        }
    }
}