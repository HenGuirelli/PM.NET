using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Benchmarks;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var streamOption = new Option<bool>(
            name: "--stream",
            description: "Benchmark stream classes.",
            getDefaultValue: () => true);

        var rootCommand = new RootCommand("PM.NET Benchmarks");

        var runCommand = new Command("run", "Run PM.NET benchmarks")
        {
            streamOption
        };

        rootCommand.AddCommand(runCommand);

        runCommand.SetHandler((stream) =>
        {
            // run benchmarks
            if (stream)
            {
                #if DEBUG
                    BenchmarkRunner.Run<PmStreamsBenchmark>(
                        DefaultConfig.Instance
                        .WithOptions(ConfigOptions.DisableOptimizationsValidator));
                # else
                    BenchmarkRunner.Run<PmStreamsBenchmark>();
                # endif
            }

        }, streamOption);

        return await rootCommand.InvokeAsync(args);
    }
}