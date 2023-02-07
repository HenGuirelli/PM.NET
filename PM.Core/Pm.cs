using System.Runtime.InteropServices;

namespace PM.Core
{
    public class Pm : IPm
    {
        private const string PmemLibHelperPath = "libpmem_helper.so";
        private const string PmemLibPath = "libpmem.so";

        private readonly IntPtr _startAddress;
        private readonly ReaderWriterLockSlim _lock = new();

        public PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; }

        public Pm(string pmMemoryMappedFilePath) : this(new PmMemoryMappedFileConfig(pmMemoryMappedFilePath)) { }

        public Pm(PmMemoryMappedFileConfig pmMemoryMappedFile)
        {
            PmMemoryMappedFileConfig = pmMemoryMappedFile;
            if (!FileExists()) CreateFile();
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
        public byte[] Load(int byteCount, int offset = 0)
        {
            var result = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                result[i] = Load(offset + (i * sizeof(byte)));
            }
            return result;
        }
        public byte Load(int offset = 0)
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
        public bool Store(byte[] values, int offset = 0)
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

        public bool Store(byte value, int offset = 0)
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

        public void DeleteFile()
        {
            File.Delete(PmMemoryMappedFileConfig.FilePath);
        }

        public bool FileExists()
        {
            return File.Exists(PmMemoryMappedFileConfig.FilePath);
        }

        public void CreateFile()
        {
            var directory = Path.GetDirectoryName(PmMemoryMappedFileConfig.FilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
            using var fs = new FileStream(PmMemoryMappedFileConfig.FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.SetLength(PmMemoryMappedFileConfig.SizeBytes);
        }

        public void Lock()
        {
            _lock.EnterWriteLock();
        }

        public void Release()
        {
            _lock.ExitWriteLock();
        }
    }
}
