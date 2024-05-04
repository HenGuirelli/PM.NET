using PM.Tests.Common;
using Xunit.Abstractions;

namespace PM.FileEngine.Tests.PMemoryTransactionFile
{
    public class TransactionFileTest : UnitTest
    {
        public TransactionFileTest(ITestOutputHelper output) 
            : base(output)
        {
        }

        [Fact]
        public void Transaction()
        {
            var streams = CreateStreamByMethodName(nameof(Transaction));


        }

        private PmStreams CreateStreamByMethodName(string methoName)
        {
            return new PmStreams
            {
                OriginalStream = CreatePmStream(methoName + "_OriginalFile"),
                TransactionStream = CreatePmStream(methoName + "_TransactionFile")
            };
        }
    }
}
