using PM.Tests.Common;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace PM.Core.Tests
{
    public class PmMemCopyStreamTests : UnitTest
    {
        public PmMemCopyStreamTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void OnWriteAndReadOnPmMemCopyStream()
        {
            using var stream = new PmMemCopyStream(
                CreateFilePath(nameof(OnWriteAndReadOnPmMemCopyStream)),
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