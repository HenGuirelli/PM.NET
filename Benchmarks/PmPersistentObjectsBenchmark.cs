using BenchmarkDotNet.Attributes;
using PM.AutomaticManager;
using PM.AutomaticManager.Configs;

namespace Benchmarks
{
    public class RootObject
    {
        public virtual string Text { get; set; }
    }

    [MemoryDiagnoser]
    [RPlotExporter]
    public class PmPersistentObjectsBenchmark
    {
        [Params(1, 2048, 4096, 8192, 16384, 32768, 65536)]
        public int OperationQuantity;

        private PersistentFactory? _persistentFactorySSD;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private RootObject _proxyPM;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        private static readonly Random _random = new();

        private const string ValueToWrite = "TextValue";

        [GlobalSetup]
        public void Setup()
        {
            var configFile = new ConfigFile();
            CleanFolder(configFile.PersistentObjectsFilePathSSD!);
            CleanFolder(configFile.PersistentObjectsFilePathPm!);

            if (configFile.PersistentObjectsFilePathSSD != null)
            {
                PmGlobalConfiguration.PmInternalsFolder = configFile.PersistentObjectsFilePathSSD!;
            }
            if (configFile.PersistentObjectsFilePathPm != null)
            {
                PmGlobalConfiguration.PmInternalsFolder = configFile.PersistentObjectsFilePathPm!;
            }

            _persistentFactorySSD = new PersistentFactory();
            _proxyPM = _persistentFactorySSD.CreateRootObject<RootObject>("RootObj");
        }

        #region ProxyObjects PM
        [Benchmark]
        public void ProxyObjects_Write_PM()
        {
            _proxyPM.Text = ValueToWrite;
        }

        [Benchmark]
        public void ProxyObjects_Read_PM()
        {
            var rand = _proxyPM.Text;
            GC.KeepAlive(rand);
        }
        #endregion

        private static void CleanFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                foreach (var filename in Directory.GetFiles(folder))
                {
                    File.Delete(filename);
                }
            }
        }
    }
}
