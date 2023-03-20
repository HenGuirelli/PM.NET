using PM.Configs;
using PM.Core.Fakes;
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

        protected static IPm CreatePm(string filepath)
        {
            if (PmGlobalConfiguration.PmTarget == PmTargets.InVolatileMemory)
            {
                return new FakeInMemoryPm(new PmMemoryMappedFileConfig(filepath));
            }
            if (PmGlobalConfiguration.PmTarget == PmTargets.PM)
            {
                return new Pm(filepath);
            }
            return new MemoryMappedFilePm(new PmMemoryMappedFileConfig(filepath));
        }


        protected static string CreateFilePath(string filename)
        {
            return Path.Combine(PmGlobalConfiguration.PmInternalsFolder, filename.EndsWith(".pm") ? filename : filename + ".pm");
        }
    }
}
