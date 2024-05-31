using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Benchmarks;
using PM.AutomaticManager.Configs;
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

        runCommand.AddCommand(streamCommand);
        runCommand.AddCommand(writeReadCommand);

        rootCommand.AddCommand(runCommand);

        writeReadCommand.SetHandler((target) =>
        {
            PmGlobalConfiguration.PmTarget = target;
#if DEBUG
            if (target == PmTargets.PM)
            {
                BenchmarkRunner.Run<PmPersistentObjectsBenchmark>(
                    DefaultConfig.Instance
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));
            }
            else if (target == PmTargets.TraditionalMemoryMappedFile)
            {
                BenchmarkRunner.Run<SSDPersistentObjectsBenchmark>(
                    DefaultConfig.Instance
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));
            }
#else
            if (target == PmTargets.PM)
            {
                BenchmarkRunner.Run<PmStreamsBenchmark>();
            }
            else if (target == PmTargets.TraditionalMemoryMappedFile)
            {
                BenchmarkRunner.Run<SSDPersistentObjectsBenchmark>();
            }
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