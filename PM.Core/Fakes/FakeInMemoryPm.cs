using System.Collections.Concurrent;

namespace PM.Core.Fakes
{
    public class FakeInMemoryPm : IPm
    {
        private static readonly ConcurrentDictionary<string, byte[]> _pmFake = new();
        public PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; }
        private readonly ReaderWriterLockSlim _lock = new();

        public FakeInMemoryPm(PmMemoryMappedFileConfig pmMemoryMappedFile)
        {
            PmMemoryMappedFileConfig = pmMemoryMappedFile;
            if (!FileExists()) CreateFile();
        }

        public byte[] Load(int byteCount, int offset = 0)
        {
            try
            {
                _lock.EnterReadLock();
                var result = new byte[byteCount];
                for (int i = offset; i < offset + byteCount; i++)
                {
                    result[i - offset] = _pmFake[PmMemoryMappedFileConfig.FilePath][i];
                }
                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public byte Load(int offset = 0)
        {
            try
            {
                _lock.EnterReadLock();
                return _pmFake[PmMemoryMappedFileConfig.FilePath][offset];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Store(byte value, int offset = 0)
        {
            try
            {
                _lock.EnterReadLock();
                _pmFake[PmMemoryMappedFileConfig.FilePath][offset] = value;
                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Store(byte[] values, int offset = 0)
        {
            try
            {
                _lock.EnterReadLock();

                for (int i = offset; i < offset + values.Length; i++)
                {
                    _pmFake[PmMemoryMappedFileConfig.FilePath][i] = values[i - offset];
                }

                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void DeleteFile()
        {
            try
            {
                _lock.EnterReadLock();
                _pmFake.TryRemove(PmMemoryMappedFileConfig.FilePath, out _);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool FileExists()
        {
            try
            {
                _lock.EnterReadLock();
                return _pmFake.ContainsKey(PmMemoryMappedFileConfig.FilePath);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public static int GetFileSize(string fileName)
        {
            return _pmFake[fileName].Length;
        }

        public void CreateFile()
        {
            try
            {
                _lock.EnterReadLock();
                if (!_pmFake.ContainsKey(PmMemoryMappedFileConfig.FilePath))
                {
                    _pmFake[PmMemoryMappedFileConfig.FilePath] = new byte[PmMemoryMappedFileConfig.SizeBytes];
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
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
