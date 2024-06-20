using PM.Tests.Common;
using Serilog.Events;
using Xunit.Abstractions;

namespace PM.Tests.Collections
{
    public class PmHashMapTest : UnitTest
    {
        public PmHashMapTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
            : base(output, logEventLevel)
        {
        }

        [Fact]
        public void On()
        {
            var pAllocator = new FileEngine.PAllocator(
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(On))),
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(On) + "_Transaction"))
                    );
            IHashMapList<int> list = new PmHashMap<int, int>(
                "PmHashMap",
                new AutomaticManager.PMemoryManager(pAllocator));

        }
    }
}
