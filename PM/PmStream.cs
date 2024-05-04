using PM.Common;
using PM.Core;
using PM.Factories;
using PM.Managers;
using Serilog;

namespace PM
{
    public class PmStream : MemoryMappedFileBasedStream, IDisposable
    {
        private readonly MemoryMappedFileBasedStream _pm;

        public override bool IsClosed { get => _pm.IsClosed; set => _pm.IsClosed = value; }

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

        public override void Resize(long size)
        {
            _pm.Resize(size);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            try
            {
                if (_pm.IsClosed)
                {
                    Log.Debug("File {file} closed on Seek. reopening file", FilePath);
                    FileHandlerManager.RegisterNewHandler(_pm);
                    _pm.Open();
                }
                return _pm.Seek(offset, origin);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{nameEx} on Seek. Try reopening file {file}", nameof(ObjectDisposedException), FilePath);
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
            _pm.Close();
            IsClosed = true;
        }

        public override void Open()
        {
            _pm.Open();
            IsClosed = false;
        }

        public void Dispose()
        {
            _pm.Dispose();
            IsClosed = true;
        }
    }
}
