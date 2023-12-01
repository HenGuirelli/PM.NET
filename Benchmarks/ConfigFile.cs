using System.Diagnostics;
using System.Text.Json;

namespace Benchmarks
{
    internal class ConfigFileContent
    {
        public string? StreamSSDFilePath { get; set; }
        public string? StreamPmFilePath { get; set; }
    }

    internal class ConfigFile
    {
        private readonly ConfigFileContent? _content;
        private readonly int _processID;
        public string? StreamSSDFilePath => _content?.StreamSSDFilePath;
        public string? StreamPmFilePath => _content?.StreamPmFilePath;

        public string? PmMarshalSSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"PmMarshalStream{_processID}.pm");
        public string? PmMemCopySSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"PmMemCopyStreamFilePath{_processID}.pm");

        public string? PmMarshalPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"PmMarshalStream{_processID}.pm");
        public string? PmMemCopyPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"PmMemCopyStreamFilePath{_processID}.pm");

        public string? MemoryMappedStreamSSDStreamFilePath => Path.Combine(StreamSSDFilePath!, $"MemoryMappedStream{_processID}.pm");
        public string? MemoryMappedStreamPmStreamFilePath => Path.Combine(StreamPmFilePath!, $"MemoryMappedStream{_processID}.pm");

        public ConfigFile()
        {
            _content = JsonSerializer.Deserialize<ConfigFileContent>(File.ReadAllText(@"benchmarks.json"));
            _processID = Process.GetCurrentProcess().Id;
        }
    }
}
