namespace PM.Core
{
    public class PmInsufficientFileSizeException : ApplicationException
    {
        public PmInsufficientFileSizeException() : base() { }
        public PmInsufficientFileSizeException(int originalSize) 
            : base($"File size {originalSize} bytes is insufficient") { }
        public PmInsufficientFileSizeException(int originalSize, int minimumSize) 
            : base($"File size {originalSize} bytes is insufficient, must be at least {minimumSize} bytes") { }
    }
}
