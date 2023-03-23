using PM.Tests.Common;
using System;
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
            var filePath = CreateFilePath(nameof(OnMapPm));
            var fileSize = 4096;
            

            var pointer = LibpmemNativeMethods.MapFile(
                path: filePath,
                length: fileSize,
                flags: Flags.PMEM_FILE_CREATE, 
                mode: 0666,
                mappedLength: out var mappedLength,
                isPersistent: out var isPersistent);

            if (pointer == IntPtr.Zero)
                throw new ApplicationException("Erro ao abrir pmem");
        }
    }
}