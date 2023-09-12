using Serilog;

namespace PM.Core.PMemory
{
    public class PMemoryAllocatedRegion
    {
        public const long DefaultFileSize = 4096;

        private readonly List<KeyValuePair<int, int>> _pointers = new();
        public IEnumerable<KeyValuePair<int, int>> Pointers => _pointers;

        private readonly PmCSharpDefinedTypes _fileBasedStream;
        private int _offset;
        private int _qtyItens;
        private int UsedSize => _qtyItens * sizeof(ulong);

        public PMemoryAllocatedRegion(PmCSharpDefinedTypes fileBasedStream)
        {
            _fileBasedStream = fileBasedStream;
            LoadFile();
        }

        public void LoadFile()
        {
            var fileSize = _fileBasedStream.FileBasedStream.Length;
            if (fileSize == 0) return;

            try
            {
                // Condition use 'i + sizeof(int) < fileSize' because 
                // always read the next int
                for (int i = 0; i + sizeof(int) < fileSize; i += sizeof(int) * 2)
                {
                    var offset = _fileBasedStream.ReadInt(i);
                    var size = _fileBasedStream.ReadInt(i + sizeof(int));
                    if (size == 0) break;
                    _pointers.Add(new KeyValuePair<int, int>(offset, size));
                }
                Log.Verbose("File={filename} loaded pointers regions: {@pointers}", _fileBasedStream.FilePath, _pointers);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "{classname} error on load pointers regions", nameof(PMemoryAllocatedRegion));
                throw;
            }
        }

        public void AddPointer(int offset, int size)
        {
            if (size == 0)
            {
                var messageError = $"{nameof(size)} cannot be zero";
                Log.Information(messageError);
                throw new ArgumentException(messageError, nameof(size));
            }

            if (_fileBasedStream.FileBasedStream.Length <= UsedSize + (2 * sizeof(int)))
            {
                var actualFileSize = _fileBasedStream.FileBasedStream.Length;
                var newSize = actualFileSize < DefaultFileSize / 2 ? DefaultFileSize : actualFileSize * 2;
                Log.Verbose(
                    "incresing size of file '{filename}' because its to low (from {actualSize} to {newSize})",
                    _fileBasedStream.FileBasedStream.FilePath, actualFileSize, newSize);
                _fileBasedStream.Resize(newSize);
            }

            _fileBasedStream.WriteInt((int)offset, _offset);
            _offset += sizeof(int);
            _fileBasedStream.WriteInt((int)size, _offset);
            _offset += sizeof(int);
            _qtyItens++;

            _pointers.Add(new KeyValuePair<int, int>(offset, size));

            Log.Verbose("New pointer allocated: offset={offset}, size={size}", offset, size);
        }

        public void ReleasePointer(int offset)
        {

        }
    }
}
