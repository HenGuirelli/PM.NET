namespace Benchmarks
{
    internal class CommandLine
    {
        private readonly string[] _args;

        public string OutputFile { get; set; }

        public CommandLine(string[] args)
        {
            _args = args;
            OutputFile = GetValueOf("-o", "--output") ?? ".";
        }

        private string? GetValueOf(params string[] keys)
        {
            try
            {
                for (int i = 0; i < _args.Length; i++)
                {
                    var arg = _args[i];
                    foreach (var key in keys)
                    {
                        if (key == arg)
                        {
                            return _args[i + 1];
                        }
                    }
                }
            }
            catch
            {
                throw new ArgumentException($"Wrong command line format to argument [{string.Join(",", keys)}]");
            }
            return null;
        }
    }
}
