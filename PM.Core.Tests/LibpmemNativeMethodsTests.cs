using PM.Tests.Common;
using System.IO;
using System.Text;
using Xunit;

namespace PM.Core.Tests
{
    public class LibpmemNativeMethodsTests : UnitTest
    {
        [Fact]
        public void OnMapPm()
        {
            LibpmemNativeMethods.MapFile(
                path: CreateFilePath(nameof(OnMapPm)),
                length: 4096,
                flags: 0, 
                mode: 0666,
                mappedLength: out var mappedLength,
                isPersistent: out var isPersistent);
        }
    }
}