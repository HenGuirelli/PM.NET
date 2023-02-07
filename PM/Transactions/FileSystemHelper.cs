using PM.Configs;

namespace PM.Transactions
{
    internal class FileSystemHelper : IFileSystemHelper
    {
        public long GetFileSize(string filename)
        {
            var fi = new FileInfo(filename);
            return fi.Length;
        }

        public IEnumerable<string> GetLogFileNames()
        {
            if (!Directory.Exists(PmGlobalConfiguration.PmTransactionFolder))
                Directory.CreateDirectory(PmGlobalConfiguration.PmTransactionFolder);

            return Directory.GetFiles(PmGlobalConfiguration.PmTransactionFolder);
        }
    }
}
