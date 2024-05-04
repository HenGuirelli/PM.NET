using PM.Common;
using Serilog;

namespace PM.Core.Factories
{
    public class PmFactory
    {
        public static PmCSharpDefinedTypes CreatePmCSharpDefinedTypes(string pmMemoryMappedFile, long size = 4096)
        {
            return new PmCSharpDefinedTypes(CreatePm(pmMemoryMappedFile, size));
        }

        public static MemoryMappedFileBasedStream CreatePm(string pmMemoryMappedFile, long size = 4096)
        {
            try
            {
                if (PmGlobalConfiguration.PmTarget == PmTargets.PM)
                {
                    return new PmMarshalStream(pmMemoryMappedFile, size);
                }
                if (PmGlobalConfiguration.PmTarget == PmTargets.TraditionalMemoryMappedFile)
                {
                    return new TraditionalMemoryMappedStream(pmMemoryMappedFile, size: size);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on create Stream {filename}", pmMemoryMappedFile);
                throw;
            }
            throw new ArgumentException(
                nameof(PmGlobalConfiguration.PmTarget));
        }
    }
}
