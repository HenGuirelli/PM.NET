using PM.Core;
using System.Diagnostics;
using System.Text.Json;

namespace Benchmarks
{
    public class PersistentObjectsBenchmarkConfig
    {
        public string PersistentObjectsFilePathSSD { get; set; } = string.Empty;
        public string PersistentObjectsFilePathPm { get; set; } = string.Empty;
        public string PersistentObjectsFilenameLiteDB { get; set; } = string.Empty;
        public string PersistentObjectsFilePathLevelDB { get; set; } = string.Empty;
        public string PostgresConnectionString { get; set; } = string.Empty;
        public string PersistentObjectsFilenameSQLite { get; set; } = string.Empty;
        public string PmTarget { get; set; } = string.Empty;
    }

    public class PmStreamsBenchmarkConfig
    {
        public string? StreamSSDFilePath { get; set; }
        public string? StreamPmFilePath { get; set; }
    }

    public class ConfigFileContent
    {
        public PmStreamsBenchmarkConfig PmStreamsBenchmark { get; set; } = new PmStreamsBenchmarkConfig();
        public PersistentObjectsBenchmarkConfig PersistentObjectsBenchmark { get; set; } = new PersistentObjectsBenchmarkConfig();
    }

    public class ConfigFile
    {
        private readonly ConfigFileContent _content;
        private readonly int _processID;
        public string? StreamSSDFilePath => _content.PmStreamsBenchmark.StreamSSDFilePath;
        public string? StreamPmFilePath => _content.PmStreamsBenchmark.StreamPmFilePath;

        public string? PmMarshalSSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"PmMarshalStream{_processID}.pm");
        public string? PmMemCopySSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"PmMemCopyStreamFilePath{_processID}.pm");

        public string? PmMarshalPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"PmMarshalStream{_processID}.pm");
        public string? PmMemCopyPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"PmMemCopyStreamFilePath{_processID}.pm");

        public string? MemoryMappedStreamSSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"MemoryMappedStream{_processID}.pm");
        public string? MemoryMappedStreamPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"MemoryMappedStream{_processID}.pm");
        public string? PersistentObjectsFilePath => Path.Combine(_content.PersistentObjectsBenchmark.PersistentObjectsFilePathSSD!, $"PersistentObjectsFilePath");
        public string? PersistentObjectsFilePathPm => Path.Combine(_content.PersistentObjectsBenchmark.PersistentObjectsFilePathPm!, $"PersistentObjectsFilePath_{_processID}");
        public PmTargets PmTarget => (PmTargets)Enum.Parse(typeof(PmTargets), _content.PersistentObjectsBenchmark.PmTarget);

        public object PersistentObjectsFilenameLiteDB => _content.PersistentObjectsBenchmark.PersistentObjectsFilenameLiteDB!;

        public string? PersistentObjectsFilePathLevelDB => Path.Combine(_content.PersistentObjectsBenchmark.PersistentObjectsFilePathLevelDB!, $"PersistentObjectsFilePath_{_processID}");

        public string? PostgresConnectionString => _content.PersistentObjectsBenchmark.PostgresConnectionString!;

        public string PersistentObjectsFilenameSQLite => _content.PersistentObjectsBenchmark.PersistentObjectsFilenameSQLite!;

        public ConfigFile()
        {
            _content = JsonSerializer.Deserialize<ConfigFileContent>(File.ReadAllText(@"benchmarks.json")) ?? throw new ApplicationException("Config file not found: benchmarks.json");
            _processID = Process.GetCurrentProcess().Id;
        }
    }
}
