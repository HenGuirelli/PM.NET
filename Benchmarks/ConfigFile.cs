using System.Diagnostics;
using System.Text.Json;

namespace Benchmarks
{
    internal class StreamConfigFileContent
    {
        public string? PmMarshalStreamFilePath { get; set; }
        public string? PmMemCopyStreamFilePath { get; set; }
    }
    internal class ConfigFileContent
    {
        public StreamConfigFileContent? Stream { get; set; }
    }

    internal class ConfigFile
    {
        private readonly ConfigFileContent? _content;
        private readonly int _processID;

        public string? PmMarshalStreamFilePath => _content?.Stream?.PmMarshalStreamFilePath?.Replace("{processID}", _processID.ToString());
        public string? PmMemCopyStreamFilePath => _content?.Stream?.PmMemCopyStreamFilePath?.Replace("{processID}", _processID.ToString());

        public ConfigFile()
        {
            _content = JsonSerializer.Deserialize<ConfigFileContent>(File.ReadAllText(@"benchmarks.json"));
            _processID = Process.GetCurrentProcess().Id;
        }
    }
}
