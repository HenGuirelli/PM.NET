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

        var collectionsCommand = new Command("collections", "Benchmark PM.NET collections")
        {
            targetOption
        };

        var writeReadCommand = new Command("writeReadProxyObject", "Benchmark write and read operations of proxy objects created by PersistentFactory")
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
        runCommand.AddCommand(collectionsCommand);

        rootCommand.AddCommand(runCommand);

        writeReadCommand.SetHandler((target) =>
        {
            PmGlobalConfiguration.PmTarget = target;
#if DEBUG
            BenchmarkRunner.Run<PersistentObjectsBenchmark>(
                DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));
#else
            BenchmarkRunner.Run<PersistentObjectsBenchmark>();
#endif

        }, targetOption);


        objectCreationCommand.SetHandler((target) =>
        {
            PmGlobalConfiguration.PmTarget = target;
#if DEBUG
            var a = new CreationObjectBenchmark();
            a.Setup();
            a.Creation();
            BenchmarkRunner.Run<CreationObjectBenchmark>(
                DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));
#else
            BenchmarkRunner.Run<CreationObjectBenchmark>();
#endif
        }, targetOption);

        collectionsCommand.SetHandler((target) =>
        {
            PmGlobalConfiguration.PmTarget = target;
#if DEBUG
            BenchmarkRunner.Run<CollectionsBenchmark>(
                DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator));
#else
            BenchmarkRunner.Run<CollectionsBenchmark>();
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