namespace PM.Core
{
    public abstract class FileBasedStorage : IPm
    {
        public PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; }
        protected readonly ReaderWriterLockSlim _lock = new();

        public FileBasedStorage(PmMemoryMappedFileConfig pmMemoryMappedFile)
        {
            PmMemoryMappedFileConfig = pmMemoryMappedFile;
            if (!FileExists()) CreateFile();
            else
            {
                var fileSize = FileSize();
                if (fileSize > PmMemoryMappedFileConfig.SizeBytes)
                {
                    PmMemoryMappedFileConfig.SizeBytes = (int)fileSize;
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
        }

        public virtual void DeleteFile()
        {
            File.Delete(PmMemoryMappedFileConfig.FilePath);
        }

        public virtual bool FileExists()
        {
            return File.Exists(PmMemoryMappedFileConfig.FilePath);
        }

        public virtual long FileSize()
        {
            using var fs = new FileStream(
                PmMemoryMappedFileConfig.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            return fs.Length;
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
