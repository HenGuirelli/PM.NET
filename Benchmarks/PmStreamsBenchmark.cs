using BenchmarkDotNet.Attributes;
using PM.Core;

namespace Benchmarks
{
    [RPlotExporter]
    public class PmStreamsBenchmark
    {
        [Params(2, 2048, 4096, 8192, 16384, 32768, 65536)]
        public int DataLength;

        private PmMarshalStream? _pmMarshalSSDStream;
        private PmMemCopyStream? _pmMemCpySSDStream;
        private PmMarshalStream? _pmMarshalPmStream;
        private PmMemCopyStream? _pmMemCpyPmStream;
        private TraditionalMemoryMappedStream? _memoryMappedStreamSSD;
        private TraditionalMemoryMappedStream? _memoryMappedStreamPm;
        private byte[] _data = Array.Empty<byte>();

        [GlobalSetup]
        public void Setup()
        {
            var configFile = new ConfigFile();
            CleanFolder(configFile.StreamSSDFilePath!);
            CleanFolder(configFile.StreamPmFilePath!);

            _data = new byte[DataLength];
            new Random(42).NextBytes(_data);

            _pmMarshalSSDStream = new PmMarshalStream(configFile.PmMarshalSSDStreamFilePath!, DataLength);
            _pmMemCpySSDStream = new PmMemCopyStream(configFile.PmMemCopySSDStreamFilePath!, DataLength);

            _pmMarshalPmStream = new PmMarshalStream(configFile.PmMarshalPmStreamFilePath!, DataLength);
            _pmMemCpyPmStream = new PmMemCopyStream(configFile.PmMemCopyPmStreamFilePath!, DataLength);

            _memoryMappedStreamSSD = new TraditionalMemoryMappedStream(configFile.MemoryMappedStreamSSDStreamFilePath!, DataLength);
            _memoryMappedStreamPm = new TraditionalMemoryMappedStream(configFile.MemoryMappedStreamPmStreamFilePath!, DataLength);
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

        #region MemoryMappedStream
        [Benchmark]
        public void MemoryMappedStream_Write_SSD()
        {
            _memoryMappedStreamSSD?.Seek(0, SeekOrigin.Begin);
            _memoryMappedStreamSSD?.Write(_data, 0, _data.Length);
        }

        [Benchmark]
        public void MemoryMappedStream_Write_Flush_SSD()
        {
            _memoryMappedStreamSSD?.Seek(0, SeekOrigin.Begin);
            _memoryMappedStreamSSD?.Write(_data, 0, _data.Length);
            _memoryMappedStreamSSD?.Flush();
        }

        [Benchmark]
        public void MemoryMappedStream_Read_SSD()
        {
            var buffer = new byte[DataLength];
            _memoryMappedStreamSSD?.Read(buffer, 0, buffer.Length);
        }

        [Benchmark]
        public void MemoryMappedStream_Write_Pm()
        {
            _memoryMappedStreamPm?.Seek(0, SeekOrigin.Begin);
            _memoryMappedStreamPm?.Write(_data, 0, _data.Length);
        }

        [Benchmark]
        public void MemoryMappedStream_Write_Flush_Pm()
        {
            _memoryMappedStreamPm?.Seek(0, SeekOrigin.Begin);
            _memoryMappedStreamPm?.Write(_data, 0, _data.Length);
            _memoryMappedStreamPm?.Flush();
        }

        [Benchmark]
        public void MemoryMappedStream_Read_Pm()
        {
            var buffer = new byte[DataLength];
            _memoryMappedStreamPm?.Read(buffer, 0, buffer.Length);
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

        [GlobalCleanup]
        public void Cleanup()
        {
            _pmMarshalSSDStream?.Dispose();
        }
    }
}
