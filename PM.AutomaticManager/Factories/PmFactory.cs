using PM.AutomaticManager.Configs;
using PM.Common;
using PM.Core;
using Serilog;
using System.IO;

namespace PM.AutomaticManager.Factories
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
                size = GetFileSizeOrDefault(pmMemoryMappedFile, size);
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

        private static long GetFileSizeOrDefault(string pmMemoryMappedFile, long @default)
        {
            if (File.Exists(pmMemoryMappedFile))
            {
                return new FileInfo(pmMemoryMappedFile).Length;
            }
            return @default;
        }
    }
}
