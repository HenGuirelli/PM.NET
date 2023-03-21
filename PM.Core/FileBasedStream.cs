namespace PM.Core
{
    public abstract class FileBasedStream : Stream
    {
        public string FilePath { get; protected set; } = string.Empty;

        internal void Delete()
        {
            Dispose();
            File.Delete(FilePath);
        }
    }
}
