﻿using Serilog;
using System.Runtime.InteropServices;

namespace PM.Common
{
    public abstract class PmBasedStream : MemoryMappedFileBasedStream
    {
        public bool IsPersistent { get; protected set; }

        private long _position;
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


        protected PmBasedStream(string path, long length)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            FilePath = path;
            _length = length;
            Open();
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

            if (File.Exists(FilePath))
            {
                length = new FileInfo(FilePath).Length;
            }

            InitialPointer = LibpmemNativeMethods.MapFile(
            path: path,
            length: length,
            flags: Flags.PMEM_FILE_CREATE,
            mode: Mode.Octal777,
            mappedLength: ref mappedLength,
            isPersistent: ref isPersistent);

            if (InitialPointer == IntPtr.Zero)
            {
                var errorMsg = LibpmemNativeMethods.ErrorMsg();
                throw new Exception("Failed to map PMEM file. Reason: " + errorMsg);
            }

            IsPersistent = isPersistent != 0;
            _length = (long)mappedLength;

            Log.Verbose(
                "PM file {filepath} with size {size} mapped into pointer {startP} to {endP} ",
                FilePath, _length, InitialPointer, InitialPointer + (nint)_length);
        }

        public override void Flush()
        {
            base.Flush();
            LibpmemNativeMethods.Flush(InitialPointer, (ulong)_length);
        }

        public override void Drain()
        {
            base.Drain();
            LibpmemNativeMethods.PmemDrain();
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
            Marshal.Copy(InitialPointer + (nint)_position, buffer, offset, buffer.Length);
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            base.LogSeek(offset, origin);

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
            var destination = InitialPointer + (nint)_position;
            Log.Verbose(
                "Writing on file={file}, size={size}, " +
                "buffer={buffer}, offset={offset}, count={count}, " +
                "destination={destination}",
                FilePath, Length,
                buffer, offset, count,
                destination);

            InternalWrite(destination, buffer, offset, count);
            _position += count;
        }

        protected abstract void InternalWrite(
            nint destination,
            byte[] buffer,
            int offset,
            int count);


        public override void Resize(long size)
        {
            base.Resize(size);
            Drain();
            Close();
            MapFile(FilePath, size);
        }

        public override void Close()
        {
            base.Close();
            IsClosed = true;

            if (InitialPointer != IntPtr.Zero)
            {
                if (LibpmemNativeMethods.Unmap(InitialPointer, Length) == 0)
                {
                    Log.Verbose("PM unmapped pointer={pointer}, filepath={filepath}, size={size}",
                        InitialPointer, FilePath, Length);
                }
                else
                {
                    Log.Error("Error on Unmap memory area pointer={pointer}, size={size}",
                        InitialPointer, Length);
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
