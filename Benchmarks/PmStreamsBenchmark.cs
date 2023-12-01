using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using PM.Core;

namespace Benchmarks
{
    [RPlotExporter]
    public class PmStreamsBenchmark
    {
        private PmMarshalStream? _pmMarshalSSDStream;
        private PmMemCopyStream? _pmMemCpySSDStream;
        private PmMarshalStream _pmMarshalPmStream;
        private PmMemCopyStream _pmMemCpyPmStream;
        private byte[]? _data;
        [Params(2, 2048, 4096, 8192, 16384, 32768, 65536)]
        public int DataLength;

        [GlobalSetup]
        public void Setup()
        {
            var configFile = new ConfigFile();

            if (!Directory.Exists(configFile.StreamSSDFilePath))
            {
                Directory.CreateDirectory(configFile.StreamSSDFilePath!);
            }
            else
            {
                foreach (var filename in Directory.GetFiles(configFile.StreamSSDFilePath))
                {
                    File.Delete(filename);
                }
            }

            _data = new byte[DataLength];
            new Random(42).NextBytes(_data);

            _pmMarshalSSDStream = new PmMarshalStream(configFile.PmMarshalSSDStreamFilePath!, DataLength);
            _pmMemCpySSDStream = new PmMemCopyStream(configFile.PmMemCopySSDStreamFilePath!, DataLength);

            _pmMarshalPmStream = new PmMarshalStream(configFile.PmMarshalPmStreamFilePath!, DataLength);
            _pmMemCpyPmStream = new PmMemCopyStream(configFile.PmMemCopyPmStreamFilePath!, DataLength);
        }

        #region PmMarshalStream
        [Benchmark]
        public void PmMarshalStream_Write_SSD()
        {
            _pmMarshalSSDStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalSSDStream?.Write(_data, 0, _data.Length);
        }

        [Benchmark]
        public void PmMarshalStream_Write_Flush_SSD()
        {
            _pmMarshalSSDStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalSSDStream?.Write(_data, 0, _data.Length);
            _pmMarshalSSDStream?.Flush();
        }

        [Benchmark]
        public void PmMarshalStream_Write_Drain_SSD()
        {
            _pmMarshalSSDStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalSSDStream?.Write(_data, 0, _data.Length);
            _pmMarshalSSDStream?.Drain();
        }

        [Benchmark]
        public void PmMarshalStream_Read_SSD()
        {
            var buffer = new byte[DataLength];
            _pmMarshalSSDStream?.Read(buffer, 0, buffer.Length);
        }


        [Benchmark]
        public void PmMarshalStream_Write_Pm()
        {
            _pmMarshalPmStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalPmStream?.Write(_data, 0, _data.Length);
        }

        [Benchmark]
        public void PmMarshalStream_Write_Flush_Pm()
        {
            _pmMarshalPmStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalPmStream?.Write(_data, 0, _data.Length);
            _pmMarshalPmStream?.Flush();
        }

        [Benchmark]
        public void PmMarshalStream_Write_Drain_Pm()
        {
            _pmMarshalPmStream?.Seek(0, SeekOrigin.Begin);
            _pmMarshalPmStream?.Write(_data, 0, _data.Length);
            _pmMarshalPmStream?.Drain();
        }

        [Benchmark]
        public void PmMarshalStream_Read_Pm()
        {
            var buffer = new byte[DataLength];
            _pmMarshalPmStream?.Read(buffer, 0, buffer.Length);
        }
        #endregion

        #region PmMemCpyStream
        [Benchmark]
        public void PmMemCpyStream_Write_SSD()
        {
            _pmMemCpySSDStream?.Seek(0, SeekOrigin.Begin);
            _pmMemCpySSDStream?.Write(_data, 0, _data.Length);
        }

        [Benchmark]
        public void PmMemCpyStream_Write_Flush_SSD()
        {
            _pmMemCpySSDStream?.Seek(0, SeekOrigin.Begin);
            _pmMemCpySSDStream?.Write(_data, 0, _data.Length);
            _pmMemCpySSDStream?.Flush();
        }

        [Benchmark]
        public void PmMemCpyStream_Write_Drain_SSD()
        {
            _pmMemCpySSDStream?.Seek(0, SeekOrigin.Begin);
            _pmMemCpySSDStream?.Write(_data, 0, _data.Length);
            _pmMemCpySSDStream?.Drain();
        }

        [Benchmark]
        public void PmMemCpyStream_Read_SSD()
        {
            var buffer = new byte[DataLength];
            _pmMemCpySSDStream?.Read(buffer, 0, buffer.Length);
        }


        [Benchmark]
        public void PmMemCpyStream_Write_Pm()
        {
            _pmMemCpyPmStream?.Seek(0, SeekOrigin.Begin);
            _pmMemCpyPmStream?.Write(_data, 0, _data.Length);
        }

        [Benchmark]
        public void PmMemCpyStream_Write_Flush_Pm()
        {
            _pmMemCpyPmStream?.Seek(0, SeekOrigin.Begin);
            _pmMemCpyPmStream?.Write(_data, 0, _data.Length);
            _pmMemCpyPmStream?.Flush();
        }

        [Benchmark]
        public void PmMemCpyStream_Write_Drain_Pm()
        {
            _pmMemCpyPmStream?.Seek(0, SeekOrigin.Begin);
            _pmMemCpyPmStream?.Write(_data, 0, _data.Length);
            _pmMemCpyPmStream?.Drain();
        }

        [Benchmark]
        public void PmMemCpyStream_Read_Pm()
        {
            var buffer = new byte[DataLength];
            _pmMemCpyPmStream?.Read(buffer, 0, buffer.Length);
        }
        #endregion

        [GlobalCleanup]
        public void Cleanup()
        {
            _pmMarshalSSDStream?.Dispose();
        }
    }
}
