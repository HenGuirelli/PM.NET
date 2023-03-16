using System.Collections.Concurrent;

namespace PM.Core
{
    public abstract class FileBasedStorage : IPm
    {
        public PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; }
        private static readonly ConcurrentDictionary<string, PmMemoryMappedFileConfig> _filesOpened
            = new();
        public long FileSize { get; }

        protected readonly ReaderWriterLockSlim _lock = new();

        public FileBasedStorage(PmMemoryMappedFileConfig pmMemoryMappedFile)
        {
            PmMemoryMappedFileConfig = pmMemoryMappedFile;
            if (!FileExists())
            {
                CreateFile();
                FileSize = pmMemoryMappedFile.SizeBytes;
            }
            else
            {
                if (!_filesOpened.ContainsKey(PmMemoryMappedFileConfig.FilePath))
                {
                    _filesOpened[PmMemoryMappedFileConfig.FilePath] = pmMemoryMappedFile;
                }
                FileSize = _filesOpened[PmMemoryMappedFileConfig.FilePath].SizeBytes;
                if (FileSize > PmMemoryMappedFileConfig.SizeBytes)
                {
                    PmMemoryMappedFileConfig.SizeBytes = (int)FileSize;
                }
            }
        }

        public virtual void CreateFile()
        {
            var directory = Path.GetDirectoryName(PmMemoryMappedFileConfig.FilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            using var fs = new FileStream(
                PmMemoryMappedFileConfig.FilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);
            fs.SetLength(PmMemoryMappedFileConfig.SizeBytes);

            _filesOpened[PmMemoryMappedFileConfig.FilePath] = PmMemoryMappedFileConfig;
        }

        public virtual void DeleteFile()
        {
            File.Delete(PmMemoryMappedFileConfig.FilePath);
        }

        public virtual bool FileExists()
        {
            return File.Exists(PmMemoryMappedFileConfig.FilePath);
        }

        public virtual long InternalGetFileSize()
        {
            using var fs = new FileStream(
                PmMemoryMappedFileConfig.FilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);
            return fs.Length;
        }

        public virtual long GetFileSize()
        {
            return FileSize;
        }

        public virtual void Resize(long sizeBytes)
        {
            using var fs = new FileStream(
                PmMemoryMappedFileConfig.FilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);
            fs.SetLength(PmMemoryMappedFileConfig.SizeBytes);
        }

        public abstract byte[] Load(int byteCount, int offset = 0);

        public abstract byte Load(int offset = 0);

        public abstract bool Store(byte value, int offset = 0);

        public abstract bool Store(byte[] values, int offset = 0);


        public virtual void Lock()
        {
            _lock.EnterWriteLock();
        }

        public virtual void Release()
        {
            _lock.ExitWriteLock();
        }
    }
}
