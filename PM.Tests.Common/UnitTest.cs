using PM.Configs;
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
    }
}
