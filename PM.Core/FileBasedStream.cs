using Serilog;

namespace PM.Core
{
    public abstract class FileBasedStream : Stream
    {
        public virtual string FilePath { get; protected set; } = string.Empty;
        public bool IsDisposed { get; protected set; }

        public virtual void Delete()
        {
            Log.Verbose("Deleting file={file}, size={size}", FilePath, Length);
            Log.CloseAndFlush();
            File.Delete(FilePath);
        }

        public virtual void Open()
        {
            Log.Verbose("Opening file={file}, size={size}", FilePath, Length);
            Log.CloseAndFlush();
        }

        public override void Flush()
        {
            Log.Verbose("Flushing file={file}, size={size}", FilePath, Length);
            Log.CloseAndFlush();
        }

        protected void LogSeek(long offset, SeekOrigin origin)
        {
            Log.Verbose(
                "Seeking on file={file}, size={size}, " +
                "offset={offset}, origin={origin}", 
                FilePath, Length,
                offset, origin);
            Log.CloseAndFlush();
        }

        public override void SetLength(long value)
        {
            Log.Verbose("SetLength called on file={file}, size={size}", FilePath, Length);
            Log.CloseAndFlush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Log.Verbose(
                "Writing on file={file}, size={size}, " +
                "buffer={buffer}, offset={offset}, count={count}",
                FilePath, Length,
                buffer, offset, count);
            Log.CloseAndFlush();
        }

        public virtual void Resize(int size)
        {
            Log.Verbose("Resizing file {file}. " +
                "Old size={oldSize}, new size={size}",
                FilePath, Length, size);
        }

        public override void Close()
        {
            Log.Verbose("Closing file {file}", FilePath);
            Log.CloseAndFlush();
        }

        protected override void Dispose(bool disposing)
        {
            Log.Verbose("Disposing file {file}", FilePath);
            Log.CloseAndFlush();
            base.Dispose(disposing);
        }
    }
}
