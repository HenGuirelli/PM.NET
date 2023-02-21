using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;

namespace PM.Core.Fakes
{
    class MemoryMappedFileItems
    {
        public string MapName { get; set; }
        public MemoryMappedFile MemoryMappedFile { get; set; }
        public MemoryMappedViewAccessor MemoryMappedViewAccessor { get; set; }
        public PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; set; }
    }

    public class MemoryMappedFilePm : IPm, IDisposable
    {
        public PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; }

        private static readonly ConcurrentDictionary<string, MemoryMappedFileItems>
            _memoryMappedFiles = new();
        private readonly string _mapName;
        private readonly ReaderWriterLockSlim _lock = new();

        public MemoryMappedFilePm(PmMemoryMappedFileConfig pmMemoryMappedFile)
        {
            PmMemoryMappedFileConfig = pmMemoryMappedFile;
            if (!FileExists()) CreateFile();
            _mapName = PmMemoryMappedFileConfig.FilePath.Replace("\\", "/");//.Replace("\\", "_").Replace("/", "_");
            if (!_memoryMappedFiles.ContainsKey(_mapName))
            {
                var mmf = MemoryMappedFile.CreateFromFile(
                    _mapName,
                    FileMode.OpenOrCreate,
                    _mapName,
                    PmMemoryMappedFileConfig.SizeBytes,
                    MemoryMappedFileAccess.ReadWrite);
                var acessor = mmf.CreateViewAccessor(0, PmMemoryMappedFileConfig.SizeBytes, MemoryMappedFileAccess.ReadWrite);

                _memoryMappedFiles[_mapName] =
                    new MemoryMappedFileItems
                    {
                        MemoryMappedFile = mmf,
                        MemoryMappedViewAccessor = acessor,
                        MapName = _mapName,
                        PmMemoryMappedFileConfig = PmMemoryMappedFileConfig
                    };
            }
            else
            {
                var oldmemoryMappedFile = _memoryMappedFiles[_mapName];
                if (oldmemoryMappedFile.PmMemoryMappedFileConfig.SizeBytes != PmMemoryMappedFileConfig.SizeBytes)
                {
                    // Resize memory mapped File
                    oldmemoryMappedFile.MemoryMappedViewAccessor.Flush();
                    oldmemoryMappedFile.MemoryMappedViewAccessor.Dispose();
                    oldmemoryMappedFile.MemoryMappedFile.Dispose();

                    SetLengthExistingFile(_mapName, PmMemoryMappedFileConfig.SizeBytes);

                    var mmf = MemoryMappedFile.CreateFromFile(
                        _mapName,
                        FileMode.OpenOrCreate,
                        _mapName,
                        0,
                        MemoryMappedFileAccess.ReadWrite);
                    var acessor = mmf.CreateViewAccessor(0, PmMemoryMappedFileConfig.SizeBytes, MemoryMappedFileAccess.ReadWrite);

                    _memoryMappedFiles[_mapName] = new MemoryMappedFileItems
                    {
                        MemoryMappedFile = mmf,
                        MemoryMappedViewAccessor = acessor,
                        MapName = _mapName,
                        PmMemoryMappedFileConfig = PmMemoryMappedFileConfig
                    };
                }
            }
        }

        public void CreateFile()
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

        private void SetLengthExistingFile(string mapName, int sizeBytes)
        {
            using var fs = new FileStream(
                mapName,
                FileMode.Open,
                FileAccess.Write,
                FileShare.None);
            fs.SetLength(sizeBytes);
        }

        public void DeleteFile()
        {
            Dispose();
            File.Delete(PmMemoryMappedFileConfig.FilePath);
        }

        public bool FileExists()
        {
            return File.Exists(PmMemoryMappedFileConfig.FilePath);
        }

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
                return _memoryMappedFiles[_mapName].MemoryMappedViewAccessor.ReadByte(offset);
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
                _memoryMappedFiles[_mapName].MemoryMappedViewAccessor.Write(offset, value);
                return true;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Store(byte[] values, int offset = 0)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Store(values[i], offset + (i * sizeof(byte)));
            }
            return true;
        }

        public void Lock()
        {
            _lock.EnterWriteLock();
        }

        public void Release()
        {
            _lock.ExitWriteLock();
        }

        public void Dispose()
        {
            foreach (var item in _memoryMappedFiles.Values)
            {
                item.MemoryMappedViewAccessor?.Dispose();
                item.MemoryMappedFile?.Dispose();
            }
        }
    }
}
