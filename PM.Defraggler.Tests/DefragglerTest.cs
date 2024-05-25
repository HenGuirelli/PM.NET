using FileFormatExplain;
using PM.FileEngine;
using PM.Tests.Common;
using Xunit.Abstractions;

namespace PM.Defraggler.Tests
{
    public class DefragglerTest : UnitTest
    {
        private readonly ITestOutputHelper _output;

        public DefragglerTest(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            var allocator = new PAllocator();

            var region1 = allocator.Alloc(4);
            var region2 = allocator.Alloc(32);
            var region3 = allocator.Alloc(64);

            region2.Write(BitConverter.GetBytes(int.MaxValue));
            region2.Free();

            var decoded = PMemoryDecoder.DecodeHex(allocator.ReadOriginalFile());
            _output.WriteLine(decoded);



            var defraggler = new Defraggler("");
            defraggler.Defrag();
        }
    }
}