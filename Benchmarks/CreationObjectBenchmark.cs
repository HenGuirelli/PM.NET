using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PM;
using PM.Configs;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    [MemoryDiagnoser]
    [RPlotExporter]
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 0, iterationCount: 2000)]
    public class CreationObjectBenchmark
    {
        private string _prefixFileName;
        private IPersistentFactory _factory;
        private const int OperationCount = 10000;

        [GlobalSetup]
        public void Setup()
        {
            var configFile = new ConfigFile();
            CleanFiles(configFile);
            SetupPmDotnet(configFile);
        }

        private void CleanFiles(ConfigFile configFile)
        {
            if (!Directory.Exists(configFile.CreationObjectBenchmarkPersistentObjectsFilePath)) return;

            foreach (var file in Directory.GetFiles(configFile.CreationObjectBenchmarkPersistentObjectsFilePath))
            {
                File.Delete(file);
            }
        }

        private void SetupPmDotnet(ConfigFile configFile)
        {
            PmGlobalConfiguration.PmTarget = configFile.CreationObjectBenchmarkPmTarget;
            PmGlobalConfiguration.PmInternalsFolder = configFile.CreationObjectBenchmarkPersistentObjectsFilePath!;

            Console.WriteLine("=====CONFIG=====");
            Console.WriteLine("PmTarget= " + PmGlobalConfiguration.PmTarget);
            Console.WriteLine("PmInternalsFolder= " + PmGlobalConfiguration.PmInternalsFolder);

            _prefixFileName = nameof(CreationObjectBenchmark);
            _factory = new PersistentFactory();
        }

        [Benchmark]
        public void CreateRootObject()
        {
            for (int i = 0; i < OperationCount; i++)
            {
                var proxyObj = _factory.CreateRootObject<ComplexClass>(_prefixFileName + Guid.NewGuid().ToString());
                GC.KeepAlive(proxyObj);
            }
        }
    }

    public class ComplexClass
    {
        public virtual PocoClass PocoObject { get; set; }
        public virtual ComplexClass SelfReferenceObject { get; set; }


        public virtual int IntVal1 { get; set; }
        public virtual int IntVal2 { get; set; }
    }

    public class PocoClass
    {
        public virtual int IntVal1 { get; set; }
        public virtual int IntVal2 { get; set; }

        public virtual long LongVal1 { get; set; }
        public virtual long LongVal2 { get; set; }

        public virtual short ShortVal1 { get; set; }
        public virtual short ShortVal2 { get; set; }

        public virtual byte ByteVal1 { get; set; }
        public virtual byte ByteVal2 { get; set; }

        public virtual double DoubleVal1 { get; set; }
        public virtual double DoubleVal2 { get; set; }

        public virtual float FloatVal1 { get; set; }
        public virtual float FloatVal2 { get; set; }

        public virtual decimal DecimalVal1 { get; set; }
        public virtual decimal DecimalVal2 { get; set; }

        public virtual string StringVal1 { get; set; }
        public virtual string StringVal2 { get; set; }

        public virtual char CharVal1 { get; set; }
        public virtual char CharVal2 { get; set; }

        public virtual bool BoolVal1 { get; set; }
        public virtual bool BoolVal2 { get; set; }
    }
}
