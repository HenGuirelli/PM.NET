using FileFormatExplain;
using PM.Collections;
using PM.Tests.Common;
using Serilog.Events;
using Xunit.Abstractions;

namespace PM.Tests.Collections
{
    public class LinkedListTests : UnitTest
    {
        private ITestOutputHelper _output;

        public LinkedListTests(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose) : base(output, logEventLevel)
        {
            _output = output;
        }

        [Fact]
        public void OnAppend_ShouldAppendValue()
        {
            var pAllocator = new FileEngine.PAllocator(
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnAppend_ShouldAppendValue))),
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnAppend_ShouldAppendValue) + "_Transaction"))
                    );
            ILinkedList<int> list = new PmLinkedList<int>(
                "PmList",
                new AutomaticManager.PMemoryManager(pAllocator));

            var content = PMemoryDecoder.DecodeHex(pAllocator.ReadOriginalFile(), dump: false, ignoreFreeRegions: true);

            var val1 = -1;
            list.Append(ref val1);

            foreach (var item in list)
            {
                _output.WriteLine("item: " + item);
            }

            content = PMemoryDecoder.DecodeHex(pAllocator.ReadOriginalFile(), dump: false, ignoreFreeRegions: true);

            Assert.Equal(list.ElementAt(0), val1);
        }

        [Fact]
        public void OnFind_ShouldFindValue()
        {
            var pAllocator = new FileEngine.PAllocator(
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnFind_ShouldFindValue))),
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnFind_ShouldFindValue) + "_Transaction"))
                    );
            ILinkedList<int> list = new PmLinkedList<int>(
                "PmList",
                new AutomaticManager.PMemoryManager(pAllocator));

            var content = PMemoryDecoder.DecodeHex(pAllocator.ReadOriginalFile(), dump: false, ignoreFreeRegions: true);

            var val1 = 3;
            list.Append(ref val1);

            Assert.Equal(0, list.Find(val1));
        }
    }
}
