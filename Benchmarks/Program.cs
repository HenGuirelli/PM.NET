#if DEBUG
using BenchmarkDotNet.Configs;
#endif
using BenchmarkDotNet.Running;
using Benchmarks;
using PM.Configs;
using PM.Core;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var targetOption = new Option<PmTargets>(
            name: "--pm-target",
            description: "Specify target to run",
            getDefaultValue: () => PmTargets.PM);

        var rootCommand = new RootCommand("PM.NET Benchmarks");

        var runCommand = new Command("run", "Run PM.NET benchmarks");

        var streamCommand = new Command("stream", "Benchmark stream classes");

        var writeReadCommand = new Command("writeReadProxyObject", "Benchmark proxy objects created by PersistentFactory")
        {
            targetOption
        };

        var objectCreationCommand = new Command("objectCreation", "Benchmark proxy objects created by PersistentFactory")
        {
            targetOption
        };

        runCommand.AddCommand(streamCommand);
        runCommand.AddCommand(writeReadCommand);
        runCommand.AddCommand(objectCreationCommand);

        rootCommand.AddCommand(runCommand);

        writeReadCommand.SetHandler((target) =>
        {
            PmGlobalConfiguration.PmTarget = target;
#if DEBUG
            BenchmarkRunner.Run<PmStreamsBenchmark>(
                DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));
#else
            BenchmarkRunner.Run<PmStreamsBenchmark>();
#endif

        }, targetOption);

        objectCreationCommand.SetHandler((target) =>
        {
            PmGlobalConfiguration.PmTarget = target;
#if DEBUG
            BenchmarkRunner.Run<CreationObjectBenchmark>(
                DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));
#else
            BenchmarkRunner.Run<CreationObjectBenchmark>();
#endif
        }, targetOption);

        streamCommand.SetHandler(() =>
        {
#if DEBUG
            BenchmarkRunner.Run<PmStreamsBenchmark>(
                DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));
#else
            BenchmarkRunner.Run<PmStreamsBenchmark>();
#endif
        });

        return await rootCommand.InvokeAsync(args);
    }
}