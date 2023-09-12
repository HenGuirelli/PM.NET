using Serilog;
using System.Runtime.InteropServices;

namespace PM.Core
{
    public class PmStream : FileBasedStream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _length;
        public bool IsPersistent { get; private set; }
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _position = value;
            }
        }


        private IntPtr _pmemPtr;
        private long _length;
        private long _position;


        public PmStream(string path, long length)
        {
            FilePath = path;
            _length = length;
            this.Open();
        }

        public override void Open()
        {
            base.Open();
            MapFile(FilePath, Length);
        }

        private void MapFile(string path, long length)
        {
            int isPersistent = 0;
            ulong mappedLength = 0;

            InitialPointer = _pmemPtr = LibpmemNativeMethods.MapFile(
                path: path,
                length: length,
                flags: Flags.PMEM_FILE_CREATE,
                mode: Mode.Octal777,
                mappedLength: ref mappedLength,
                isPersistent: ref isPersistent);


            if (_pmemPtr == IntPtr.Zero)
            {
                throw new Exception("Failed to map PMEM file.");
            }

            IsPersistent = isPersistent != 0;
            _length = (long)mappedLength;

            Log.Verbose(
                "PM file {filepath} with size {size} mapped into pointer {startP} to {endP} ",
                FilePath, _length, _pmemPtr, _pmemPtr + (nint)_length);
        }

        public override void Flush()
        {
            base.Flush();
            LibpmemNativeMethods.Flush(_pmemPtr, (ulong)_length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position + count > _length)
            {
                count = (int)(_length - _position);
            }
            if (count <= 0)
            {
                return 0;
            }
            Marshal.Copy(_pmemPtr + (nint)_position, buffer, offset, buffer.Length);
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            base.LogSeek(offset, origin);
            if (IsClosed)
            {
                throw new ObjectDisposedException($"file {FilePath} disposed");
            }

            long newPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            if (newPosition < 0 || newPosition > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            _position = newPosition;
            return _position;
        }

        public override void SetLength(long value)
        {
            base.SetLength(value);
            if (value < 0 || value > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var destination = _pmemPtr + (nint)_position;
            Log.Verbose(
                "Writing on file={file}, size={size}, " +
                "buffer={buffer}, offset={offset}, count={count}, " +
                "destination={destination}",
                FilePath, Length,
                buffer, offset, count,
                destination);
            Marshal.Copy(buffer, offset, destination, count);
            _position += count;
        }

        public override void Resize(long size)
        {
            base.Resize(size);

            Close();
            MapFile(FilePath, size);
        }

        public override void Close()
        {
            base.Close();
            IsClosed = true;

            if (_pmemPtr != IntPtr.Zero)
            {
                if (LibpmemNativeMethods.Unmap(_pmemPtr, Length) == 0)
                {
                    Log.Verbose("PM unmapped pointer={pointer}, filepath={filepath}, size={size}",
                        _pmemPtr, FilePath, Length);
                }
                else
                {
                    Log.Error("Error on Unmap memory area pointer={pointer}, size={size}",
                        _pmemPtr, Length);
                }
            }
            else
            {
                Log.Verbose("Unable to close PM filepath={filepath}, size={size}. _pmPtr is Zero", FilePath, Length);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Close();
        }
    }
}
