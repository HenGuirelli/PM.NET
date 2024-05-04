namespace PM.FileEngine.Tests.PMemoryTransactionFile
{
    public class PmStreams
    {
        public Core.MemoryMappedFileBasedStream OriginalStream { get; set; }
        public Core.MemoryMappedFileBasedStream TransactionStream { get; set; }
    }
}
