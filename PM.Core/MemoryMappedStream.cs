using System.IO.MemoryMappedFiles;

namespace PM.Core
{
    public class MemoryMappedStream : FileBasedStream
    {
        private MemoryMappedFile _memoryMappedFile;
        private MemoryMappedViewStream _memoryMappedViewStream;
        private readonly long _size;
        private readonly bool _createFileIfNotExists;

        public MemoryMappedStream(string filePath, long size, bool createFileIfNotExists = true)
        {
            FilePath = filePath;
            _size = size;
            _createFileIfNotExists = createFileIfNotExists;
            Open();
        }

        private void Open()
        {
            if (_createFileIfNotExists && !File.Exists(FilePath))
            {
                using var fs = new FileStream(
                    FilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None);
                fs.SetLength(_size);
            }
            _memoryMappedFile = MemoryMappedFile.CreateFromFile(FilePath);
            _memoryMappedViewStream =
                _memoryMappedFile.CreateViewStream(0, _size, MemoryMappedFileAccess.ReadWrite);
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _memoryMappedViewStream.Length;

        public override long Position
        {
            get => _memoryMappedViewStream.Position;
            set => _memoryMappedViewStream.Position = value;
        }

        public override void Flush()
        {
            _memoryMappedViewStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = _memoryMappedViewStream.Read(buffer, offset, count);
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            try
            {
                return _memoryMappedViewStream.Seek(offset, origin);
            }
            catch (ObjectDisposedException)
            {
                Open();
                return Seek(offset, origin);
            }
        }

        public override void SetLength(long value)
        {
            _memoryMappedViewStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                _memoryMappedViewStream.Write(buffer, offset, count);
            }
            catch (System.NotSupportedException ex)
            {

            }
        }


        public override void Resize(int size)
        {
            Close();

            var fs = new FileStream(
                FilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
            fs.SetLength(size);
            fs.Dispose();

            _memoryMappedFile = MemoryMappedFile.CreateFromFile(FilePath);
            _memoryMappedViewStream = _memoryMappedFile.CreateViewStream(0, size, MemoryMappedFileAccess.ReadWrite);
        }

        public override void Close()
        {
            _memoryMappedViewStream?.Dispose();
            _memoryMappedFile?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Close();
            IsDisposed = true;
        }
    }
}
