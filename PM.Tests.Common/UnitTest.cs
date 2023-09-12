using PM.Configs;
using PM.Core;
using System.IO;
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

        protected static void DeleteFile(string filename)
        {
            var filepath = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, filename);
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        protected static void DeleteAllFilesFromFolder(string folder)
        {
            foreach (string file in Directory.EnumerateFiles(folder))
            {
                File.Delete(file);
            }
        }
    }
}
