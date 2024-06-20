using PM.Collections;
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
        public void OnPut_ShouldAddElementToHashMap()
        {
            var pAllocator = new FileEngine.PAllocator(
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnPut_ShouldAddElementToHashMap))),
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnPut_ShouldAddElementToHashMap) + "_Transaction"))
                    );
            IHashMap<int, int> hashmap = new PmHashMap<int, int>(
                "PmHashMap",
                new AutomaticManager.PMemoryManager(pAllocator));

            hashmap.Put(1, 1);
            hashmap.Put(2, 1);
            hashmap.Put(3, 3);

            Assert.Equal(1, hashmap.Get(1));
            Assert.Equal(1, hashmap.Get(2));
            Assert.Equal(3, hashmap.Get(3));
        }
    }
}
