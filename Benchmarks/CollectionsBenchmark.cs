using BenchmarkDotNet.Attributes;
using PM.AutomaticManager;
using PM.Collections;
using PM.Common;
using PM.FileEngine;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    [MemoryDiagnoser]
    [RPlotExporter]
    public class CollectionsBenchmark
    {
        [Params(2, 2048, 4096/*, 8192, 16384, 32768, 65536*/)]
        public int DataLength;
        private IHashMap<string, int> _hashMapStringInt;

        private static readonly Random _random = new Random();
        private PmLinkedList<int> _linkedList;

        private readonly string _hashMapKey = Guid.NewGuid().ToString();
        private readonly string[] _hashMapKeys = new string[10];
        private const int PreDefinedItensQtty = 10;

        [GlobalSetup]
        public void Setup()
        {
            var pAllocator = new PAllocator(
                    new PmCSharpDefinedTypes(new TraditionalMemoryMappedStream($"1{nameof(CollectionsBenchmark)}_{DataLength}_TraditionalMemoryMappedStream", 4096)),
                    new PmCSharpDefinedTypes(new TraditionalMemoryMappedStream($"1{nameof(CollectionsBenchmark)}_{DataLength}_TraditionalMemoryMappedStream_Transaction", 4096))
            );

            _hashMapStringInt = new PmHashMap<string, int>(Guid.NewGuid().ToString(), new PMemoryManager(pAllocator));
            // Put one item to GetBenchmark
            _hashMapStringInt.Put(_hashMapKey, _random.Next());

            _linkedList = new PmLinkedList<int>(Guid.NewGuid().ToString(), new PMemoryManager(pAllocator));
            // Add itens to search benchmark
            for (int i = 0; i < PreDefinedItensQtty; i++)
            {
                var val = i;
                _linkedList.Append(ref i);
                _hashMapKeys[i] = Guid.NewGuid().ToString();
            }
        }

        [Benchmark]
        public void HashMap_String_Int_PutNewItem()
        {
            _hashMapStringInt.Put(Guid.NewGuid().ToString(), _random.Next());
        }

        [Benchmark]
        public void HashMap_String_Int_PutOverride()
        {
            var existingItem = _hashMapKeys[_random.Next(PreDefinedItensQtty)];
            _hashMapStringInt.Put(existingItem, _random.Next());
        }

        [Benchmark]
        public void HashMap_Get()
        {
            var existingItem = _hashMapKeys[_random.Next(PreDefinedItensQtty)];
            _hashMapStringInt.Get(existingItem);
        }

        [Benchmark]
        public void LnkedList_Append()
        {
            var val = _random.Next();
            _linkedList.Append(ref val);
        }

        [Benchmark]
        public void LnkedList_Get()
        {
            var valueToFind = _random.Next(10);
            var indexValue = _linkedList.Find(valueToFind);

            GC.KeepAlive(indexValue);
        }
    }
}
