using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using PM.Core;

namespace Benchmarks
{
    [RPlotExporter]
    public class PmStreamsBenchmark
    {
        private PmMarshalStream? _pmMarshalStream;
        private PmMemCopyStream? _pmMemCpyStream;

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
            _pmMemCpyStream  = new PmMemCopyStream(@$"./benchmarks/PmMemCpyStream{nProcessID}.pm", DataLength);
        }

#region PmMarshalStream
        [Benchmark]
        public void PmMarshalStream_Write()
        {
            _pmMarshalStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalStream?.Write(_data, 0, _data.Length);
        }

        [Benchmark]
        public void PmMarshalStream_Write_Flush()
        {
            _pmMarshalStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalStream?.Write(_data, 0, _data.Length);
            _pmMarshalStream?.Flush();
        }

        [Benchmark]
        public void PmMarshalStream_Read()
        {
            var buffer = new byte[DataLength];
            _pmMarshalStream?.Read(buffer, 0, buffer.Length);
        }
#endregion

#region PmMemCpyStream
        [Benchmark]
        public void PmMemCpyStream_Write()
        {
            _pmMemCpyStream?.Seek(0, SeekOrigin.Begin);
            _pmMemCpyStream?.Write(_data, 0, _data.Length);
        }

        [Benchmark]
        public void PmMemCpyStream_Write_Flush()
        {
            _pmMemCpyStream?.Seek(0, SeekOrigin.Begin);
            _pmMemCpyStream?.Write(_data, 0, _data.Length);
            _pmMemCpyStream?.Flush();
        }

        [Benchmark]
        public void PmMemCpyStream_Read()
        {
            var buffer = new byte[DataLength];
            _pmMemCpyStream?.Read(buffer, 0, buffer.Length);
        }
#endregion

        [GlobalCleanup]
        public void Cleanup()
        {
            _pmMarshalStream?.Dispose();
        }
    }
}
