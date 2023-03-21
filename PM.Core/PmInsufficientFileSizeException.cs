namespace PM.Core
{
    public class PmInsufficientFileSizeException : ApplicationException
    {
        public PmInsufficientFileSizeException() : base() { }
        public PmInsufficientFileSizeException(long originalSize) 
            : base($"File size {originalSize} bytes is insufficient") { }
        public PmInsufficientFileSizeException(long originalSize, long minimumSize) 
            : base($"File size {originalSize} bytes is insufficient, must be at least {minimumSize} bytes") { }
    }
}
