﻿using PM.AutomaticManager.Configs;
using PM.Common;
using PM.Core;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace PM.Tests.Common
{
    [Collection("PM.UnitTests")]
    public abstract class UnitTest
    {
        public ITestOutputHelper Output { get; }
        static UnitTest()
        {
            PmGlobalConfiguration.PmTarget = Constraints.PmTarget;
            PmGlobalConfiguration.PmInternalsFolder = Constraints.PmRootFolder;
        }

        public UnitTest(ITestOutputHelper output, Serilog.Events.LogEventLevel logEventLevel = Serilog.Events.LogEventLevel.Verbose)
        {
            Output = output;
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logEventLevel)
            .WriteTo.TestOutput(Output)
            .CreateLogger();
        }

        protected static void ClearFolder()
        {
            foreach (var filename in Directory.GetFiles(PmGlobalConfiguration.PmInternalsFolder))
            {
                File.Delete(filename);
            }
        }

        protected static string CreateFilePath(string filename)
        {
            return Path.Combine(PmGlobalConfiguration.PmInternalsFolder, filename);
        }

        protected static MemoryMappedFileBasedStream CreatePmStream(string mappedMemoryFilePath, long size = 4096)
        {
            if (PmGlobalConfiguration.PmTarget == PmTargets.PM)
            {
                return new PmMemCopyStream(CreateFilePath(mappedMemoryFilePath), size);
            }
            return new TraditionalMemoryMappedStream(CreateFilePath(mappedMemoryFilePath), size);
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
