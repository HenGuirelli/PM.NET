using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Benchmarks;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var outputDirectoryOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Output directory path.");
        var targetOption = new Option<string>(
            aliases: new[] { "--target", "-t" },
            description: "Memory technology target: libpmem/mmf.",
            getDefaultValue: () => "libpmem");
        var streamOption = new Option<bool>(
            name: "--stream",
            description: "Benchmark stream classes.",
            getDefaultValue: () => true);

        var rootCommand = new RootCommand("PM.NET Benchmarks");

        var runCommand = new Command("run", "Run PM.NET benchmarks")
        {
            outputDirectoryOption,
            targetOption,
            streamOption
        };

        rootCommand.AddCommand(runCommand);

        runCommand.SetHandler((outputDirectory, target, stream) =>
        {
            // run benchmarks
            if (stream)
            {
                # if DEBUG
                    BenchmarkRunner.Run<PmStreamsBenchmark>(
                        DefaultConfig.Instance
                        .WithOptions(ConfigOptions.DisableOptimizationsValidator));
                # else
                    BenchmarkRunner.Run<PmStreamsBenchmark>();
                # endif
            }

        }, outputDirectoryOption, targetOption, streamOption);

        return await rootCommand.InvokeAsync(args);
    }
}