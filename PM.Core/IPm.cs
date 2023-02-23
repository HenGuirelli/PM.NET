namespace PM.Core
{
    public interface IPm
    {
        PmMemoryMappedFileConfig PmMemoryMappedFileConfig { get; }

        byte[] Load(int byteCount, int offset = 0);
        byte Load(int offset = 0);
        bool Store(byte value, int offset = 0);
        bool Store(byte[] values, int offset = 0);

        void CreateFile();
        void DeleteFile();
        bool FileExists();
        long FileSize();

        void Lock();
        void Release();
    }
}