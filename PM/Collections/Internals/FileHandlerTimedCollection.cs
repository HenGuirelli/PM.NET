using PM.Core;
using PM.Managers;
using System.Collections.Concurrent;

namespace PM.Collections.Internals
{
    public class LimitedDictItem<TKey, TValue>
    {
        public TValue Value { get; }
        public TKey Key { get; }
        public DateTime AddedDatetime { get; } = DateTime.Now;
        public DateTime LastGetDatetime { get; private set; } = DateTime.Now;

        public LimitedDictItem(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        internal void UpdateLastGetTime(List<LimitedDictItem<TKey, TValue>> items)
        {
            LastGetDatetime = DateTime.Now;
            items.Remove(this);
            items.Add(this);
        }
    }

    public class FileHandlerTimedCollection
    {
        public int Capacity { get; set; }
        public int Count => _fileHandlersByFilename.Count;
        private readonly ConcurrentDictionary<string, LimitedDictItem<string, FileHandlerItem>>
            _fileHandlersByFilename = new();
        private readonly List<LimitedDictItem<string, FileHandlerItem>>
            _orderedFileHandlers = new();

        public FileHandlerTimedCollection(int capacity)
        {
            Capacity = capacity;
        }

        public void Add(string key, FileHandlerItem value)
        {
            if (Count >= Capacity)
            {
                throw new ApplicationException("Capacity limit reached");
            }
            var dictItem = new LimitedDictItem<string, FileHandlerItem>(key, value);
            _orderedFileHandlers.Add(dictItem);
            if (!_fileHandlersByFilename.TryAdd(key, dictItem))
            {
                throw new Exception("Error on add item " + key);
            }
        }

        public bool TryGetValue(string key, out FileHandlerItem value)
        {
            if (_fileHandlersByFilename.TryGetValue(key, out var dictItem))
            {
                value = dictItem.Value;
                dictItem.UpdateLastGetTime(_orderedFileHandlers);
                return true;
            }
            value = default;
            return false;
        }

        public bool TryRemove(string key, out FileHandlerItem value)
        {
            if (_fileHandlersByFilename.TryRemove(key, out var dictItem))
            {
                value = dictItem.Value;

                return true;
            }
            value = default;
            return false;
        }

        public IEnumerable<FileBasedStream> CleanOldValues(int qtyToClean)
        {
            var removedItems = new List<FileBasedStream>();
            for (int i = 0; i < qtyToClean; i++)
            {
                LimitedDictItem<string, FileHandlerItem> itemToRemove = _orderedFileHandlers[i];
                _orderedFileHandlers.Remove(itemToRemove);
                _fileHandlersByFilename.Remove(itemToRemove.Key, out var _);

                removedItems.Add(itemToRemove.Value.FileBasedStream);
            }
            return removedItems;
        }
    }
}
