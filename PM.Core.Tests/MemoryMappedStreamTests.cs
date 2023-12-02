using PM.Tests.Common;
using System.IO;
using System.Text;
using Xunit;

namespace PM.Core.Tests
{
    public class MemoryMappedStreamTests : UnitTest
    {
        [Fact]
        public void OnMemoryMappedStreamTests()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello, world!");

            using var stream = new TraditionalMemoryMappedStream(
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
