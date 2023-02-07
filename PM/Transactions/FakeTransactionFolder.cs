using PM.Core.Fakes;

namespace PM.Transactions
{
    internal class FakeTransactionFolder : IFileSystemHelper
    {
        public long GetFileSize(string filename)
        {
            return FakeInMemoryPm.GetFileSize(filename);
        }

        public IEnumerable<string> GetLogFileNames()
        {
            return Enumerable.Empty<string>();
        }
    }
}
