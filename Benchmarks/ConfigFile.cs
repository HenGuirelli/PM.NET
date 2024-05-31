using PM.AutomaticManager.Configs;
using PM.Core;
using System.Diagnostics;
using System.Text.Json;

namespace Benchmarks
{
    internal class ConfigFileContent
    {
        public string? StreamSSDFilePath { get; set; }
        public string? StreamPmFilePath { get; set; }
        public string PersistentObjectsFilePathSSD { get; set; } = string.Empty;
        public string PersistentObjectsFilePathPm { get; set; } = string.Empty;
        public string PersistentObjectsFilenameLiteDB { get; set; } = string.Empty;
        public string PersistentObjectsFilePathLevelDB { get; set; } = string.Empty;
        public string PostgresConnectionString { get; set; } = string.Empty;
    }

    internal class ConfigFile
    {
        private readonly ConfigFileContent _content;
        private readonly int _processID;
        public string? StreamSSDFilePath => _content?.StreamSSDFilePath;
        public string? StreamPmFilePath => _content?.StreamPmFilePath;

        public string? PmMarshalSSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"PmMarshalStream{_processID}.pm");
        public string? PmMemCopySSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"PmMemCopyStreamFilePath{_processID}.pm");

        public string? PmMarshalPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"PmMarshalStream{_processID}.pm");
        public string? PmMemCopyPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"PmMemCopyStreamFilePath{_processID}.pm");

        public string? MemoryMappedStreamSSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"MemoryMappedStream{_processID}.pm");
        public string? MemoryMappedStreamPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"MemoryMappedStream{_processID}.pm");
        public string? PersistentObjectsFilePathSSD => Path.Combine(_content.PersistentObjectsFilePathSSD!, $"PersistentObjectsFilePath_{_processID}");
        public string? PersistentObjectsFilePathPm => Path.Combine(_content.PersistentObjectsFilePathPm!, $"PersistentObjectsFilePath_{_processID}");
        public PmTargets PmTarget => PmGlobalConfiguration.PmTarget;

        public object PersistentObjectsFilenameLiteDB => _content.PersistentObjectsFilenameLiteDB!;

        public string? PersistentObjectsFilePathLevelDB => Path.Combine(_content.PersistentObjectsFilePathLevelDB!, $"PersistentObjectsFilePath_{_processID}");

        public string? PostgresConnectionString => _content.PostgresConnectionString!;

        public ConfigFile()
        {
            _content = JsonSerializer.Deserialize<ConfigFileContent>(File.ReadAllText(@"benchmarks.json")) ?? throw new ApplicationException("Config file not found: benchmarks.json");
            _processID = Process.GetCurrentProcess().Id;
        }
    }
}
