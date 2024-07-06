using BenchmarkDotNet.Attributes;
using PM;
using PM.Configs;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    [MemoryDiagnoser]
    [RPlotExporter]
    public class CreationObjectBenchmark
    {
        [Params(2, 2048, 4096)]
        public int CreationsQty;
        private string _prefixFileName;
        private IPersistentFactory _factory;

        [GlobalSetup]
        public void Setup()
        {
            var configFile = new ConfigFile();
            SetupPmDotnet(configFile);
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
        public void Creation()
        {
            for (int i = 0; i < CreationsQty; i++)
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
