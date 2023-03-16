using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;

namespace PM.Core.Fakes
{
    public class MemoryMappedFilePm : FileBasedStorage, IDisposable
    {
        private class MemoryMappedFileItems
        {
            public string MapName { get; set; }
            public MemoryMappedFile MemoryMappedFile { get; set; }
            public MemoryMappedViewAccessor MemoryMappedViewAccessor { get; set; }
            public PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; set; }
        }

        private static readonly ConcurrentDictionary<string, MemoryMappedFileItems>
            _memoryMappedFiles = new();
        private readonly string _mapName;

        public MemoryMappedFilePm(PmMemoryMappedFileConfig pmMemoryMappedFile)
            : base(pmMemoryMappedFile)
        {
            _mapName = PmMemoryMappedFileConfig.FilePath.Replace("\\", "_").Replace("/", "_").Replace(":", "_");
            if (!_memoryMappedFiles.ContainsKey(_mapName))
            {
                CreateMemoryMappedFile();
            }
            else
            {
                Resize(PmMemoryMappedFileConfig.SizeBytes);
            }
        }

        public override void Resize(long sizeBytes)
        {
            var oldmemoryMappedFile = _memoryMappedFiles[_mapName];
            if (oldmemoryMappedFile.PmMemoryMappedFileConfig.SizeBytes != sizeBytes)
            {
                // Resize memory mapped File
                oldmemoryMappedFile.MemoryMappedViewAccessor.Flush();
                oldmemoryMappedFile.MemoryMappedViewAccessor.Dispose();
                oldmemoryMappedFile.MemoryMappedFile.Dispose();

                SetLengthExistingFile(PmMemoryMappedFileConfig.FilePath, sizeBytes);
                PmMemoryMappedFileConfig.SizeBytes = (int)sizeBytes;
                CreateMemoryMappedFile();
            }
        }

        private void CreateMemoryMappedFile()
        {
            var mmf = MemoryMappedFile.CreateFromFile(
                                    PmMemoryMappedFileConfig.FilePath,
                                    FileMode.OpenOrCreate,
                                    _mapName,
                                    0,
                                    MemoryMappedFileAccess.ReadWrite);

            var acessor = mmf.CreateViewAccessor(
                0,
                PmMemoryMappedFileConfig.SizeBytes,
                MemoryMappedFileAccess.ReadWrite);

            _memoryMappedFiles[_mapName] = new MemoryMappedFileItems
            {
                MemoryMappedFile = mmf,
                MemoryMappedViewAccessor = acessor,
                MapName = _mapName,
                PmMemoryMappedFileConfig = PmMemoryMappedFileConfig
            };
        }


        private void SetLengthExistingFile(string fileName, long sizeBytes)
        {
            using var fs = new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Write,
                FileShare.None);
            fs.SetLength(sizeBytes);
        }

        public override void DeleteFile()
        {
            Dispose();
            base.DeleteFile();
        }

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
                return _memoryMappedFiles[_mapName].MemoryMappedViewAccessor.ReadByte(offset);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override bool Store(byte value, int offset = 0)
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

        public override bool Store(byte[] values, int offset = 0)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Store(values[i], offset + (i * sizeof(byte)));
            }

            if (_memoryMappedFiles.TryGetValue(_mapName, out var mmf))
            {
                mmf.MemoryMappedViewAccessor.Flush();
            }

            return true;
        }

        public void Dispose()
        {
            if (_memoryMappedFiles.TryGetValue(_mapName, out var mmf))
            {
                mmf.MemoryMappedViewAccessor.Flush();
                mmf.MemoryMappedViewAccessor?.Dispose();
                mmf.MemoryMappedFile?.Dispose();
                _memoryMappedFiles.TryRemove(_mapName, out _);
            }
        }
    }
}
