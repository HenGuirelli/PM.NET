using System.Diagnostics;
using System.Text.Json;

namespace Benchmarks
{
    internal class ConfigFileContent
    {
        public string? StreamFilePath { get; set; }
    }

    internal class ConfigFile
    {
        private readonly ConfigFileContent? _content;
        private readonly int _processID;
        public string? StreamFolder => _content?.StreamFilePath;

        public string? PmMarshalStreamFilePath => Path.Combine(StreamFolder!, $"PmMarshalStream{_processID}.pm");
        public string? PmMemCopyStreamFilePath => Path.Combine(StreamFolder!, $"PmMemCopyStreamFilePath{_processID}.pm");

        public ConfigFile()
        {
            _content = JsonSerializer.Deserialize<ConfigFileContent>(File.ReadAllText(@"benchmarks.json"));
            _processID = Process.GetCurrentProcess().Id;
        }
    }
}
