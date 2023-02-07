using PM.Core;
using PM.Configs;
using PM.Core.Fakes;

namespace PM.Factories
{
    public class PmFactory
    {
        public static IPm CreatePm(PmMemoryMappedFileConfig pmMemoryMappedFile)
        {
            if (PmGlobalConfiguration.PmTarget == PmTargets.PM)
            {
                return new Pm(pmMemoryMappedFile);
            }
            if (PmGlobalConfiguration.PmTarget == PmTargets.TraditionalMemoryMappedFile)
            {
                return new MemoryMappedFilePm(pmMemoryMappedFile);
            }
            return new FakeInMemoryPm(pmMemoryMappedFile);
        }
    }
}
