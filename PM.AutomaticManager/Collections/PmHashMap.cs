using PM.AutomaticManager;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace PM.Collections
{
    internal class PmKeyValuePairEqualityComparer<TKey, TValue>
        : IEqualityComparer<PmKeyValuePair<TKey, TValue>>
    {
        // TODO: not work if key is a complex object
        public bool Equals(PmKeyValuePair<TKey, TValue>? x, PmKeyValuePair<TKey, TValue>? y)
        {
            if (x is null && y != null) return false;
            if (y is null && x != null) return false;
            if (x is null && y is null) return true;

            return x.Key.GetHashCode() == y.Key.GetHashCode();
        }

        public int GetHashCode([DisallowNull] PmKeyValuePair<TKey, TValue> obj)
        {
            return obj.Key.GetHashCode();
        }
    }

    public class PmHashMap<TKey, TValue> : IHashMap<TKey, TValue>
    {
        private int _size;
        public PmLinkedList<PmKeyValuePair<TKey, TValue>>[] _buckets;
        private readonly string _objectUserID;
        private readonly PMemoryManager _pMemoryManager;
        private readonly PmKeyValuePairEqualityComparer<TKey, TValue> _equalityComparer;

        public PmHashMap(
            string objectUserID,
            PMemoryManager pMemoryManager,
            int size = 100)
        {
            _size = size;
            _buckets = new PmLinkedList<PmKeyValuePair<TKey, TValue>>[size];
            _objectUserID = objectUserID;
            _pMemoryManager = pMemoryManager;
            _equalityComparer = new PmKeyValuePairEqualityComparer<TKey, TValue>();
        }

        public TValue? Get(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            var indexBucket = GetIndexBucket(key);
            if (_buckets[indexBucket] == null) return default;

            var keyToFind = new PmKeyValuePair<TKey, TValue>(key, default!);
            var linkedListIndex = _buckets[indexBucket].Find(keyToFind);
            if (linkedListIndex == -1) return default;
            return _buckets[indexBucket].ElementAt(linkedListIndex).Value;
        }

        private int GetIndexBucket(TKey key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            return Math.Abs(key.GetHashCode() % _size);
        }

        public void Put(TKey key, TValue value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (value is null) throw new ArgumentNullException(nameof(value));

            var indexBucket = GetIndexBucket(key);
            if (_buckets[indexBucket] == null)
            {
                _buckets[indexBucket] = new PmLinkedList<PmKeyValuePair<TKey, TValue>>(
                    BuildBucketKey(indexBucket),
                    _pMemoryManager,
                    _equalityComparer);
            }

            var item = new PmKeyValuePair<TKey, TValue>(key, value);
            var indexItemFindend = _buckets[indexBucket].Find(item);
            if (indexItemFindend != -1)
            {
                _buckets[indexBucket].RemoveAt(indexItemFindend);
            }

            _buckets[indexBucket].Append(ref item);
        }

        private string BuildBucketKey(int bucketIndex)
        {
            return $"{_objectUserID}_{bucketIndex}";
        }

        public IEnumerator<PmKeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
