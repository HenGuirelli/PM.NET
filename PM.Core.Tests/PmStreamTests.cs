using PM.Tests.Common;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace PM.Core.Tests
{
    public class PmStreamTests : UnitTest
    {
        [Fact]
        public void OnWriteAndReadOnStream()
        {
            using var stream = new PmStream(
                CreateFilePath(nameof(OnWriteAndReadOnStream)),
                4096);

            var bytesToWrite = Encoding.UTF8.GetBytes("Hello PM!");
            stream.Write(bytesToWrite);
            stream.Flush();

            var buffer = new byte[bytesToWrite.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            Assert.Equal("Hello PM!", Encoding.UTF8.GetString(buffer));
        }
    }
}