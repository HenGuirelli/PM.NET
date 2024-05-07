using Serilog;
using System.IO.MemoryMappedFiles;

namespace PM.Common
{
    public class TraditionalMemoryMappedStream : MemoryMappedFileBasedStream
    {
        public override long Position
        {
            get => _memoryMappedViewStream!.Position;
            set => _memoryMappedViewStream!.Position = value;
        }

        private MemoryMappedFile? _memoryMappedFile;
        private MemoryMappedViewStream? _memoryMappedViewStream;

        public TraditionalMemoryMappedStream(string filePath, long size)
        {
            FilePath = filePath;
            _length = size;
            Open();
        }

        public override void Open()
        {
            base.Open();
            if (!File.Exists(FilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                using var fs = new FileStream(
                    FilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None);
                fs.SetLength(_length);
            }

            _memoryMappedFile = MemoryMappedFile.CreateFromFile(FilePath);
            _memoryMappedViewStream =
                _memoryMappedFile.CreateViewStream(0, _length, MemoryMappedFileAccess.ReadWrite);

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
            _memoryMappedViewStream!.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _memoryMappedViewStream!.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            base.LogSeek(offset, origin);
            return _memoryMappedViewStream!.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            base.SetLength(value);
            _memoryMappedViewStream!.SetLength(value);
        }

        private int _recursionCount = 0;
        public override void Write(byte[] buffer, int offset, int count)
        {
            //try
            //{
                Log.Verbose(
                    "Writing on file={file}, size={size}, " +
                    "buffer={buffer}, offset={offset}, count={count}",
                    FilePath, Length,
                    buffer, offset, count);
                _memoryMappedViewStream!.Write(buffer, offset, count);
            //}
            //catch (System.NotSupportedException ex)
            //    when (ex.Message.Contains("Unable to expand length of this stream beyond its capacity."))
            //{
            //    if (_recursionCount == 1) throw;

            //    Resize(_length * 2);

            //    _recursionCount++;
            //    Write(buffer, offset, count);
            //    _recursionCount = 0;
            //}
        }

        public override void Resize(long size)
        {
            base.Resize(size);

            Flush();

            Close();
            _length = size;

            using (var fs = new FileStream(
                                    FilePath,
                                    FileMode.Open,
                                    FileAccess.Write,
                                    FileShare.None))
            {
                fs.SetLength(_length);
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
                    _memoryMappedViewStream!.SafeMemoryMappedViewHandle.ReleasePointer();
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
