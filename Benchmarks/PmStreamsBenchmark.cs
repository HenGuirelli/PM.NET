using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using PM.Core;

namespace Benchmarks
{
    [RPlotExporter]
    public class PmStreamsBenchmark
    {
        private PmMarshalStream? _pmMarshalStream;

        private byte[]? _data;
        [Params(2, 2048, 4096, 8192, 16384, 32768, 65536)]
        public int DataLength;

        [GlobalSetup]
        public void Setup()
        {
            if (!Directory.Exists(@"./benchmarks"))
            {
                Directory.CreateDirectory(@"./benchmarks");
            }
            else
            {
                foreach(var filename in Directory.GetFiles(@"./benchmarks"))
                {
                    File.Delete(filename);
                }
            }
            
            _data = new byte[DataLength];
            new Random(42).NextBytes(_data);

            int nProcessID = Process.GetCurrentProcess().Id;
            _pmMarshalStream = new PmMarshalStream(@$"./benchmarks/PmMarshalStream{nProcessID}.pm", DataLength);
        }

        [Benchmark]
        public void PmMarshalStream_Write()
        {
            _pmMarshalStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalStream?.Write(_data, 0, _data.Length);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pmMarshalStream?.Dispose();
        }
    }
}
