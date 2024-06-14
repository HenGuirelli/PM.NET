using BenchmarkDotNet.Attributes;
using PM;
using PM.Configs;

namespace Benchmarks
{
    public class RootObject
    {
        public virtual int IntVal { get; set; }

        public virtual long LongVal { get; set; }

        public virtual short ShortVal { get; set; }

        public virtual byte ByteVal { get; set; }

        public virtual double DoubleVal { get; set; }

        public virtual float FloatVal { get; set; }

        public virtual decimal DecimalVal { get; set; }

        public virtual char CharVal { get; set; }

        public virtual bool BoolVal { get; set; }

        public virtual string StringVal { get; set; }
        public virtual RootObject InnerObject { get; set; }
    }

    [MemoryDiagnoser]
    [RPlotExporter]
    public class PmStreamsBenchmark
    {
        private byte[] _data = Array.Empty<byte>();
        private IPersistentFactory _persistentFactorySSD;
        private RootObject _proxy;

        [GlobalSetup]
        public void Setup()
        {
            var configFile = new ConfigFile();
            CleanFolder(configFile.StreamSSDFilePath!);
            CleanFolder(configFile.StreamPmFilePath!);
            SetupPmDotnet(configFile);
        }

        private void SetupPmDotnet(ConfigFile configFile)
        {
            PmGlobalConfiguration.PmTarget = configFile.PmTarget;
            PmGlobalConfiguration.PmInternalsFolder = configFile.PersistentObjectsFilePath!;

            _persistentFactorySSD = new PersistentFactory();
            _proxy = _persistentFactorySSD.CreateRootObject<RootObject>("RootObj");
        }

        [Benchmark]
        [WarmupCount(20)]
        [IterationCount(300)]
        public void ProxyObjects_CreationRootObject()
        {
            var root = _persistentFactorySSD.CreateRootObject<RootObject>(Guid.NewGuid().ToString());
            GC.KeepAlive(root);
        }

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

        [GlobalCleanup]
        public void Cleanup()
        {
        }
    }
}
