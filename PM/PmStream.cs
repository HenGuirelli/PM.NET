using PM.Core;
using PM.Factories;
using PM.Managers;
using Serilog;

namespace PM
{
    public class PmStream : FileBasedStream, IDisposable
    {
        private readonly FileBasedStream _pm;

        public override string FilePath => _pm.FilePath;

        public PmStream(string filepath, long size = 4096)
        {
            _pm = PmFactory.CreatePm(filepath, size);
        }

        public override bool CanRead => _pm.CanRead;

        public override bool CanSeek => _pm.CanSeek;

        public override bool CanWrite => _pm.CanWrite;

        public override long Length => _pm.Length;

        public override long Position
        {
            get { return _pm.Position; }
            set { _pm.Position = value; }
        }

        public override void Flush()
        {
            _pm.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _pm.Read(buffer, offset, count);
        }

        public override void Resize(int size)
        {
            _pm.Resize(size);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            try
            {
                return _pm.Seek(offset, origin);
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error(ex, "ObjectDisposedException on Seek. reopening file {file}", FilePath);
                FileHandlerManager.RegisterNewHandler(_pm);
                _pm.Open();
                return _pm.Seek(offset, origin);
            }
        }

        public override void SetLength(long value)
        {
            _pm.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _pm.Write(buffer, offset, count);
        }

        public override void Close()
        {
            Dispose();
        }

        public override void Open()
        {
            _pm.Open();
        }

        public void Dispose()
        {
            _pm.Close();
            _pm.Dispose();
            IsDisposed = true;
        }
    }
}
