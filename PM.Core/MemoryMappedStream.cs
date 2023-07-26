using Serilog;
using System.IO.MemoryMappedFiles;
using System.Diagnostics;

namespace PM.Core
{
    public class MemoryMappedStream : FileBasedStream
    {

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _size;
        public override long Position
        {
            get => _memoryMappedViewStream.Position;
            set => _memoryMappedViewStream.Position = value;
        }


        private MemoryMappedFile _memoryMappedFile;
        private MemoryMappedViewStream _memoryMappedViewStream;
        private long _size;
        private readonly bool _createFileIfNotExists;


        public MemoryMappedStream(string filePath, long size, bool createFileIfNotExists = true)
        {
            FilePath = filePath;
            _size = size;
            _createFileIfNotExists = createFileIfNotExists;
            Open();
        }

        public override void Open()
        {
            base.Open();
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

        public override void Flush()
        {
            base.Flush();
            _memoryMappedViewStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _memoryMappedViewStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            base.LogSeek(offset, origin);
            return _memoryMappedViewStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            base.SetLength(value);
            _memoryMappedViewStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            _memoryMappedViewStream.Write(buffer, offset, count);
        }

        public override void Resize(int size)
        {
            base.Resize(size);

            Close();

            var fs = new FileStream(
                FilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
            fs.SetLength(size);
            fs.Dispose();
            _size = size;

            _memoryMappedFile = MemoryMappedFile.CreateFromFile(FilePath);
            _memoryMappedViewStream = _memoryMappedFile.CreateViewStream(0, size, MemoryMappedFileAccess.ReadWrite);
        }

        public override void Close()
        {
            base.Close();

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
