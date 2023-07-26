using Serilog;
using System.Diagnostics;
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
            MapFile(path, length);

            if (_pmemPtr == IntPtr.Zero)
            {
                throw new Exception("Failed to map PMEM file.");
            }
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

            _pmemPtr = LibpmemNativeMethods.MapFile(
                path: path,
                length: length,
                flags: Flags.PMEM_FILE_CREATE,
                mode: Mode.Octal777,
                mappedLength: ref mappedLength,
                isPersistent: ref isPersistent);

            IsPersistent = isPersistent != 0;
            _length = (long)mappedLength;
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
            _position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
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
            base.Write(buffer, offset, count);
            Marshal.Copy(buffer, offset, _pmemPtr + (nint)_position, count);
            _position += count;
        }

        public override void Resize(int size)
        {
            base.Resize(size);

            Close();
            MapFile(FilePath, size);
        }

        public override void Close()
        {
            base.Close();

            if (_pmemPtr != IntPtr.Zero)
            {
                LibpmemNativeMethods.Unmap(_pmemPtr, _length);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Close();
            IsDisposed = true;
        }
    }
}
