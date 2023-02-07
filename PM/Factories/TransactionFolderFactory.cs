using PM.Configs;
using PM.Transactions;

namespace PM.Factories
{
    internal class TransactionFolderFactory
    {
        public static IFileSystemHelper Create()
        {
            if (PmGlobalConfiguration.PmTarget == PmTargets.PM ||
                PmGlobalConfiguration.PmTarget == PmTargets.TraditionalMemoryMappedFile)
            {
                return new FileSystemHelper();
            }
            return new FakeTransactionFolder();
        }
    }
}
