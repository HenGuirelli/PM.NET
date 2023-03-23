using PM.Core;
using PM.Configs;

namespace PM.Factories
{
    public class PmFactory
    {
        public static FileBasedStream CreatePm(string pmMemoryMappedFile, long size = 4096)
        {
            if (PmGlobalConfiguration.PmTarget == PmTargets.PM)
            {
                return new PmStream(pmMemoryMappedFile, size);
            }
            if (PmGlobalConfiguration.PmTarget == PmTargets.TraditionalMemoryMappedFile)
            {
                return new MemoryMappedStream(pmMemoryMappedFile, size: size);
            }
            throw new ArgumentException(
                nameof(PmGlobalConfiguration.PmTarget));
        }
    }
}
