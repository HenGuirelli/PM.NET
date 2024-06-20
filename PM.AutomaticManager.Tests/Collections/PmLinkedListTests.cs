using FileFormatExplain;
using PM.Collections;
using PM.Tests.Common;
using Serilog.Events;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace PM.Tests.Collections
{
    public class PmLinkedListTests : UnitTest
    {
        private ITestOutputHelper _output;

        public PmLinkedListTests(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose) : base(output, logEventLevel)
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


        [Fact]
        public void OnAppendAndFind_WhenComplexObjects_ShouldAppendAndFindValue()
        {
            var pAllocator = new FileEngine.PAllocator(
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnAppendAndFind_WhenComplexObjects_ShouldAppendAndFindValue))),
                    new PM.Common.PmCSharpDefinedTypes(CreatePmStream(nameof(OnAppendAndFind_WhenComplexObjects_ShouldAppendAndFindValue) + "_Transaction"))
                    );
            ILinkedList<MyKeyValuePair<string, int>> list = new PmLinkedList<MyKeyValuePair<string, int>>(
                "PmList",
                new AutomaticManager.PMemoryManager(pAllocator),
                new MyKeyValuePair<string, int>());

            var content = PMemoryDecoder.DecodeHex(pAllocator.ReadOriginalFile(), dump: false, ignoreFreeRegions: true);

            var val1 = new MyKeyValuePair<string, int>("key1", 1);
            var val2 = new MyKeyValuePair<string, int>("key2", 2);
            var val3 = new MyKeyValuePair<string, int>("key3", 3);
            list.Append(ref val1);
            list.Append(ref val2);
            list.Append(ref val3);

            Assert.Equal(0, list.Find(val1));
            Assert.Equal(1, list.Find(val2));
            Assert.Equal(2, list.Find(val3));
        }
    }

    public class MyKeyValuePair<TKey, TValue> : IEqualityComparer<MyKeyValuePair<TKey, TValue>>
    {
        public virtual TKey Key { get; set; }
        public virtual TValue Value { get; set; }

        public MyKeyValuePair() { }

        public MyKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public bool Equals(MyKeyValuePair<TKey, TValue>? x, MyKeyValuePair<TKey, TValue>? y)
        {
            if (x is null && y is null) return true;

            if (x is null || y is null) return false;

            return x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
        }

        public int GetHashCode([DisallowNull] MyKeyValuePair<TKey, TValue> obj)
        {
            return unchecked(Key?.GetHashCode() ?? 1 * Value?.GetHashCode() ?? 1);
        }
    }
}
