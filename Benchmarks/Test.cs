using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [MemoryDiagnoser]
    [RPlotExporter]
    public class Test
    {
        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public int Test1()
        {
            var a = 1;
            var b = 2;
            return a + b;
        }
    }
}
