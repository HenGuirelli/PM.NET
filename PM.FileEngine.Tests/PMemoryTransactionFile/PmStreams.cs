using PM.Common;

namespace PM.FileEngine.Tests.PMemoryTransactionFile
{
    public class PmStreams
    {
        public MemoryMappedFileBasedStream OriginalStream { get; set; }
        public MemoryMappedFileBasedStream TransactionStream { get; set; }
    }
}
