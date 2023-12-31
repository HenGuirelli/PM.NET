﻿using Serilog;

namespace PM.Core.PMemory
{
    /// <summary>
    /// Region of persistent memory.
    /// </summary>
    public class PersistentRegion
    {
        /// <summary>
        /// Define the region is free or in use.
        /// </summary>
        internal bool IsFree { get; set; }

        /// <summary>
        /// Start pointer to region
        /// </summary>
        public int Pointer { get; set; }

        /// <summary>
        /// Region size in bytes
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Index of region inside a block
        /// </summary>
        internal int RegionIndex { get; set; }

        private readonly PmCSharpDefinedTypes _persistentMemory;

        public PersistentRegion(PmCSharpDefinedTypes persistentMemory, int size)
        {
            Size = size;
            _persistentMemory = persistentMemory;
        }

        public byte[] GetData(int count, int offset)
        {
            if (count + offset > Size)
            {
                var exceptionMessage = $"Attempt to read in other pmemory region. " +
                    $"Read attempt: {count} bytes in address {Pointer + offset} | " +
                    $"Region limit: {Pointer + Size}";
                var ex = new AccessViolationException(exceptionMessage);
                Log.Error(ex, exceptionMessage);
                throw ex;
            }

            return _persistentMemory.ReadBytes(count, offset + Pointer);
        }

        public void Write(byte[] value, int offset)
        {
            if (value.Length + offset > Size)
            {
                var exceptionMessage = $"Attempt to write in other pmemory region. " +
                    $"Write attempt: {value.Length} bytes in address {Pointer + offset} | " +
                    $"Region limit: {Pointer + Size}";
                var ex = new AccessViolationException(exceptionMessage);
                Log.Error(ex, exceptionMessage);
                throw ex;
            }

            Log.Verbose("Writing @bytes bytes in address @address (region offset @offset)", value, offset + Pointer, offset);
            _persistentMemory.WriteBytes(value, offset + Pointer);
        }
    }
}
