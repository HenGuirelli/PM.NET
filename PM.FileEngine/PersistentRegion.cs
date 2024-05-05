using PM.Common;
using PM.FileEngine;
using PM.FileEngine.Transactions;
using Serilog;

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
        public uint Pointer { get; set; }

        /// <summary>
        /// Region size in bytes
        /// </summary>
        public uint Size { get; }

        /// <summary>
        /// Index of region inside a block
        /// </summary>
        public int RegionIndex { get; set; }

        private readonly PmCSharpDefinedTypes _persistentMemory;
        private readonly PersistentBlockLayout _persistentBlockLayout;
        private readonly TransactionFile _transactionFile;

        public PersistentRegion(
            PmCSharpDefinedTypes persistentMemory,
            TransactionFile transactionFile,
            uint size, PersistentBlockLayout persistentBlockLayout)
        {
            Size = size;
            _persistentMemory = persistentMemory;
            _persistentBlockLayout = persistentBlockLayout;
            _transactionFile = transactionFile;
        }

        public byte[] Read(uint? count = null, int? offset = null)
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

            return _persistentMemory.ReadBytes(count ?? Size, (offset ?? 0) + Pointer);
        }

        public void Write(byte[] value, int? offset = null)
        {
            if (value.Length + (offset ?? 0) > Size)
            {
                var exceptionMessage = $"Attempt to write in other pmemory region. " +
                    $"Write attempt: {value.Length} bytes in address {Pointer + offset} | " +
                    $"Region limit: {Pointer + Size}";
                var ex = new AccessViolationException(exceptionMessage);
                Log.Error(ex, exceptionMessage);
                throw ex;
            }

            Log.Verbose(
                "Writing {bytes} bytes in address={address} (region offset={offset} | Region={regionIndex} | Block={blockID})",
                value, (offset ?? 0) + Pointer, offset, RegionIndex, _persistentBlockLayout.BlockOffset);
            //_persistentMemory.WriteBytes(value, (offset ?? 0) + Pointer);
            _transactionFile.AddUpdateContentBlockLayout(
                new UpdateContentBlockLayout(
                    startBlockOffset: (uint)((offset ?? 0) + Pointer),
                    contentSize: (uint)value.Length,
                    content: value));
        }
    }
}
