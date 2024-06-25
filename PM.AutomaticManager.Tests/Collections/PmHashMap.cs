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

        [Fact]
        public void OnPut_LoadTest_ShouldAddElementToHashMap()
        {
            const int qty = 10000;

            var pAllocator = new FileEngine.PAllocator(
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnPut_LoadTest_ShouldAddElementToHashMap))),
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnPut_LoadTest_ShouldAddElementToHashMap) + "_Transaction"))
                    );
            IHashMap<string, int> hashmap = new PmHashMap<string, int>(
                "PmHashMap",
                new AutomaticManager.PMemoryManager(pAllocator));

            for (int i = 0; i < qty; i++)
            {
                hashmap.Put(Guid.NewGuid().ToString(), i);
            }
        }

        [Fact]
        public void OnPut_WithStringKey_ShouldAddElementToHashMap()
        {
            var pAllocator = new FileEngine.PAllocator(
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnPut_WithStringKey_ShouldAddElementToHashMap))),
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnPut_WithStringKey_ShouldAddElementToHashMap) + "_Transaction"))
                    );
            IHashMap<string, int> hashmap = new PmHashMap<string, int>(
                nameof(OnPut_WithStringKey_ShouldAddElementToHashMap) + "PmHashMap",
                new AutomaticManager.PMemoryManager(pAllocator));

            hashmap.Put("Key 1", 1);
            hashmap.Put("Key 2", 1);
            hashmap.Put("Key 3", 3);

            Assert.Equal(1, hashmap.Get("Key 1"));
            Assert.Equal(1, hashmap.Get("Key 2"));
            Assert.Equal(3, hashmap.Get("Key 3"));

            // Overide 
            hashmap.Put("Key 3", 4);
            Assert.Equal(4, hashmap.Get("Key 3"));
        }

        [Fact]
        public void OnPut_WithStringKeyAndOverride_ShouldReplaceElementToHashMap()
        {
            var pAllocator = new FileEngine.PAllocator(
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnPut_WithStringKeyAndOverride_ShouldReplaceElementToHashMap))),
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnPut_WithStringKeyAndOverride_ShouldReplaceElementToHashMap) + "_Transaction"))
                    );
            IHashMap<string, int> hashmap = new PmHashMap<string, int>(
                nameof(OnPut_WithStringKeyAndOverride_ShouldReplaceElementToHashMap) + "PmHashMap",
                new AutomaticManager.PMemoryManager(pAllocator));

            hashmap.Put("Key 1", 1);
            hashmap.Put("Key 2", 1);
            hashmap.Put("Key 3", 3);

            Assert.Equal(1, hashmap.Get("Key 1"));
            Assert.Equal(1, hashmap.Get("Key 2"));
            Assert.Equal(3, hashmap.Get("Key 3"));

            // Overide 
            hashmap.Put("Key 2", 4);
            Assert.Equal(4, hashmap.Get("Key 2"));
        }
    }
}
