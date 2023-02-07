using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;

namespace PM.Core.Fakes
{
    public class MemoryMappedFilePm : IPm, IDisposable
    {
        private readonly MemoryMappedFile _mmf;
        private readonly MemoryMappedViewAccessor _acessor;

        public PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; }

        private static readonly ConcurrentDictionary<string, List<(MemoryMappedFile, MemoryMappedViewAccessor)>>
            _memoryMappedFiles = new();
        private readonly string _mapName;
        private readonly ReaderWriterLockSlim _lock = new();

        public MemoryMappedFilePm(PmMemoryMappedFileConfig pmMemoryMappedFile)
        {
            PmMemoryMappedFileConfig = pmMemoryMappedFile;
            if (!FileExists()) CreateFile();
            _mapName = PmMemoryMappedFileConfig.FilePath.Replace("\\", "_").Replace("/", "_");
            try
            {
                _mmf = MemoryMappedFile.CreateFromFile(
                    PmMemoryMappedFileConfig.FilePath,
                    FileMode.OpenOrCreate,
                    _mapName);
            }
            catch (IOException)
            {
                _mmf = MemoryMappedFile.OpenExisting(_mapName);
            }
            _acessor = _mmf.CreateViewAccessor(0, PmMemoryMappedFileConfig.SizeBytes);
            var result = _memoryMappedFiles.GetOrAdd(_mapName, _ => new());
            result.Add((_mmf, _acessor));
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
                return _acessor.ReadByte(offset);
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
                _acessor.Write(offset, value);
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
            foreach (var item in _memoryMappedFiles[_mapName])
            {
                item.Item1?.Dispose();
                item.Item2?.Dispose();
            }
        }
    }
}
