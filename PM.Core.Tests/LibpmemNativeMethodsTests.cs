using PM.Tests.Common;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
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
        
        [Fact]
        public void WriteAndReadFromPmem()
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

            var content = Encoding.UTF8.GetBytes("Hello World");
            // Write
            Marshal.Copy(content, 0, pointer, content.Length);

            // Read
            var res = new byte[content.Length];
            Marshal.Copy(pointer, res, 0, res.Length);
            var resultstr = Encoding.UTF8.GetString(res);
            Assert.Equal("Hello World", resultstr);
        }
    }
}