using PM.Configs;
using PM.Core;
using Xunit;

namespace PM.Tests.Common
{
    [Collection("PM.UnitTests")]
    public abstract class UnitTest
    {
        protected UnitTest()
        {
            PmGlobalConfiguration.PmTarget = Constraints.PmTarget;
            PmGlobalConfiguration.PmInternalsFolder = Constraints.PmRootFolder;
        }

        protected static string CreateFilePath(string filename)
        {
            return Path.Combine(PmGlobalConfiguration.PmInternalsFolder, filename);
        }

        protected static FileBasedStream CreatePmStream(string mappedMemoryFilePath, long size)
        {
            if (PmGlobalConfiguration.PmTarget == PmTargets.PM)
            {
                return new PmStream(CreateFilePath(mappedMemoryFilePath), size);
            }
            return new MemoryMappedStream(CreateFilePath(mappedMemoryFilePath), size);
        }
    }
}
