using Serilog;

namespace PM.FileEngine
{
    public abstract class MemoryMappedFileBasedStream : Stream
    {
        public virtual string FilePath { get; protected set; } = string.Empty;
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;

        protected long _length;
        public override long Length => _length;


        private bool _isClosed = true;
        public virtual bool IsClosed
        {
            get => _isClosed;
            set => _isClosed = value;
        }
        public IntPtr InitialPointer { get; protected set; }

        public virtual void Delete()
        {
            Log.Verbose("Deleting file={file}, size={size}", FilePath, Length);
            File.Delete(FilePath);
        }

        public virtual void Open()
        {
            Log.Verbose("Opening file={file}, size={size}", FilePath, Length);
            IsClosed = false;
        }

        public override void Flush()
        {
            Log.Verbose("Flushing file={file}, size={size}", FilePath, Length);
        }

        public virtual void Drain()
        {
            Log.Verbose("Drain PM");
        }

        protected void LogSeek(long offset, SeekOrigin origin)
        {
            Log.Verbose(
                "Seeking on file={file}, size={size}, " +
                "offset={offset}, origin={origin}",
                FilePath, Length,
                offset, origin);
        }

        public override void SetLength(long value)
        {
            Log.Verbose("SetLength called on file={file}, size={size}", FilePath, Length);
        }

        public virtual void Resize(long size)
        {
            Log.Verbose("Resizing file {file}. " +
                "Old size={oldSize}, new size={size}",
                FilePath, Length, size);
        }

        public override void Close()
        {
            Log.Verbose("Closing file {file}", FilePath);
        }

        protected override void Dispose(bool disposing)
        {
            Log.Verbose("Disposing file {file}", FilePath);
            base.Dispose(disposing);
        }
    }
}
