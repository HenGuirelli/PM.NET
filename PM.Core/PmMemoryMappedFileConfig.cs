namespace PM.Core
{
    public class PmMemoryMappedFileConfig
    {
        /// <summary>
        /// Indicates the path of the memory-mapped file.
        /// </summary>
        public string FilePath { get; }
        /// <summary>
        /// Indicates the total size in bytes of the memory-mapped file.
        /// </summary>
        public int SizeBytes { get; }
        public const int DefaultPmFileSizeBytes = 4096;

        public PmMemoryMappedFileConfig(string name, int size = DefaultPmFileSizeBytes)
        {
            FilePath = name;
            SizeBytes = size;
        }
    }
}
