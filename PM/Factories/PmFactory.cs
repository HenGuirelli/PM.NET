using PM.Core;
using PM.Configs;
using Serilog;

namespace PM.Factories
{
    public class PmFactory
    {
        public static FileBasedStream CreatePm(string pmMemoryMappedFile, long size = 4096)
        {
            try
            {
                if (PmGlobalConfiguration.PmTarget == PmTargets.PM)
                {
                    return new Core.PmStream(pmMemoryMappedFile, size);
                }
                if (PmGlobalConfiguration.PmTarget == PmTargets.TraditionalMemoryMappedFile)
                {
                    return new MemoryMappedStream(pmMemoryMappedFile, size: size);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on create Stream");
                throw;
            }
            throw new ArgumentException(
                nameof(PmGlobalConfiguration.PmTarget));
        }
    }
}
