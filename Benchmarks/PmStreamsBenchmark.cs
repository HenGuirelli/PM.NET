using BenchmarkDotNet.Attributes;
using PM.Core;

namespace Benchmarks
{
    [RPlotExporter]
    public class PmStreamsBenchmark
    {
        private PmMarshalStream _pmMarshalStream;
        public static string PmMarshalStreamPath { get; set; }

        private byte[] _data;
        [Params(1, 2048, 4096)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            _data = new byte[N];
            new Random(42).NextBytes(_data);

            _pmMarshalStream = new PmMarshalStream(PmMarshalStreamPath, 4096);
        }


        [Benchmark]
        public void PmMarshalStream_Write()
        {
            _pmMarshalStream.Write(_data, 0, _data.Length);
        }
    }
}
