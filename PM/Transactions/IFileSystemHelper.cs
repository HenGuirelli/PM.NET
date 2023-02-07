namespace PM.Transactions
{
    internal interface IFileSystemHelper
    {
        IEnumerable<string> GetLogFileNames();
        long GetFileSize(string path);
    }
}
