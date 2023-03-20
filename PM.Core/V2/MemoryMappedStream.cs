using System.IO.MemoryMappedFiles;

namespace PM.Core.V2
{
    public class MemoryMappedStream : Stream
    {
        private readonly MemoryMappedFile _memoryMappedFile;
        private readonly MemoryMappedViewStream _memoryMappedViewStream;

        public MemoryMappedStream(string filePath, long size, long offset = 0, bool createFileIfNotExists = true)
        {
            if (createFileIfNotExists && !File.Exists(filePath))
            {
                using var fs = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None);
                fs.SetLength(size);
            }
            _memoryMappedFile = MemoryMappedFile.CreateFromFile(filePath);
            _memoryMappedViewStream = _memoryMappedFile.CreateViewStream(offset, size, MemoryMappedFileAccess.ReadWrite);
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
            Position = 0;
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _memoryMappedViewStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _memoryMappedViewStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _memoryMappedViewStream.Write(buffer, offset, count);
            Position = 0;
        }

        protected override void Dispose(bool disposing)
        {
            _memoryMappedViewStream?.Dispose();
            _memoryMappedFile?.Dispose();
            base.Dispose(disposing);
        }
    }
}
