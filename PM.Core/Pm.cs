using System.Runtime.InteropServices;

namespace PM.Core
{
    public class Pm : FileBasedStorage
    {
        private const string PmemLibHelperPath = "libpmem_helper.so";
        private const string PmemLibPath = "libpmem.so";

        private readonly IntPtr _startAddress;

        public Pm(string pmMemoryMappedFilePath) : this(new PmMemoryMappedFileConfig(pmMemoryMappedFilePath)) { }

        public Pm(PmMemoryMappedFileConfig pmMemoryMappedFile)
            : base(pmMemoryMappedFile)
        {
            _startAddress = MemoryMapFile();
        }

        #region Load
        [DllImport(PmemLibHelperPath, EntryPoint = "pmem_read_bytes")]
        private static extern IntPtr _ReadBytes(string path, int fileSize);
        private IntPtr MemoryMapFile()
        {
            return _ReadBytes(PmMemoryMappedFileConfig.FilePath, PmMemoryMappedFileConfig.SizeBytes);
        }

        [DllImport(PmemLibHelperPath, EntryPoint = "pmemclr_pmem_memcpy_persist")]
        private static extern IntPtr _PmMemcpyPersist(IntPtr address, int sizeBytes);
        public override byte[] Load(int byteCount, int offset = 0)
        {
            var result = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                result[i] = Load(offset + (i * sizeof(byte)));
            }
            return result;
        }
        public override byte Load(int offset = 0)
        {
            try
            {
                _lock.EnterReadLock();
                return Marshal.ReadByte(_PmMemcpyPersist(_startAddress + offset, sizeof(byte)));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        #endregion

        #region Store
        [DllImport(PmemLibPath, EntryPoint = "pmem_memset_persist")]
        private static extern bool PmMemsetPersist(IntPtr address, int value, int count);
        public override bool Store(byte[] values, int offset = 0)
        {
            int count = 0;
            foreach (var value in values)
            {
                if (!Store(value, offset: offset + (count++ * sizeof(byte))))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Store(byte value, int offset = 0)
        {
            try
            {
                _lock.EnterReadLock();
                return PmMemsetPersist(_startAddress + offset, value, 1);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        #endregion
    }
}
