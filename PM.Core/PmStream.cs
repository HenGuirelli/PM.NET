using System.Runtime.InteropServices;

namespace PM.Core
{
    public class PmStream : FileBasedStream
    {
        private readonly IntPtr _pmemPtr;
        private long _length;
        private long _position;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _length;

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

        public PmStream(string path, long length)
        {
            _pmemPtr = LibpmemNativeMethods.MapFile(path, length, 0666, 0, out _length, out int isPersistent);
            if (_pmemPtr == IntPtr.Zero)
            {
                throw new Exception("Failed to map PMEM file.");
            }
        }

        public override void Flush()
        {
            LibpmemNativeMethods.Flush(_pmemPtr, new UIntPtr((ulong)_length));
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
            //Marshal.Copy(_pmemPtr + _position, buffer, offset, count);
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
            if (value < 0 || value > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_position + count > _length)
            {
                _length = _position + count;
            }
            //Marshal.Copy(buffer, offset, _pmemPtr + _position, count);
            _position += count;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_pmemPtr != IntPtr.Zero)
            {
                LibpmemNativeMethods.Unmap(_pmemPtr, _length);
            }
        }

        public override void Resize(int size)
        {
            throw new NotImplementedException();
        }
    }
}
