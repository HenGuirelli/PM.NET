namespace PM.Core
{
    public abstract class FileBasedStream : Stream
    {
        public virtual string FilePath { get; protected set; } = string.Empty;

        public bool IsDisposed { get; protected set; }

        public virtual void Delete()
        {
            File.Delete(FilePath);
        }

        public abstract void Resize(int size);
    }
}
