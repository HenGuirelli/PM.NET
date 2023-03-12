using PM.Configs;

namespace PM.Tests.Common
{
    public abstract class UnitTest
    {
        protected UnitTest()
        {
            PmGlobalConfiguration.PmTarget = Constraints.PmTarget;
            PmGlobalConfiguration.PmInternalsFolder = Constraints.PmRootFolder;
        }
    }
}
