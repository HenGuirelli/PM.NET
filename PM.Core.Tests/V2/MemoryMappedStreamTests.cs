using PM.Core.V2;
using PM.Tests.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PM.Core.Tests.V2
{
    public class MemoryMappedStreamTests : UnitTest
    {
        [Fact]
        public void OnMemoryMappedStreamTests()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello, world!");

            using var stream = new MemoryMappedStream(
                CreateFilePath(nameof(OnMemoryMappedStreamTests)),
                data.Length);

            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Position = 0;
            byte[] buffer = new byte[data.Length];
            int bytesRead = stream.Read(buffer, 0, data.Length);

            Assert.Equal("Hello, world!", Encoding.UTF8.GetString(buffer));
        }
    }
}
