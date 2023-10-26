using Serilog;
using System.IO.MemoryMappedFiles;

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

            unsafe
            {
                byte* pointer = null;
                _memoryMappedViewStream.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                InitialPointer = (IntPtr)pointer;
            }
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
            Log.Verbose(
                "Writing on file={file}, size={size}, " +
                "buffer={buffer}, offset={offset}, count={count}",
                FilePath, Length,
                buffer, offset, count);
            _memoryMappedViewStream.Write(buffer, offset, count);
        }

        public override void Resize(long size)
        {
            base.Resize(size);

            Flush();

            Close();
            _size = size;

            using (var fs = new FileStream(
                                    FilePath,
                                    FileMode.Open,
                                    FileAccess.Write,
                                    FileShare.None))
            {
                fs.SetLength(_size);
            }

            Open();
        }

        public override void Close()
        {
            if (!IsClosed)
            {
                base.Close();

                if (InitialPointer != IntPtr.Zero)
                {
                    _memoryMappedViewStream.SafeMemoryMappedViewHandle.ReleasePointer();
                    InitialPointer = IntPtr.Zero;
                }

                _memoryMappedViewStream?.Close();
                _memoryMappedViewStream?.Dispose();
                _memoryMappedFile?.Dispose();



                IsClosed = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Close();
        }
    }
}
