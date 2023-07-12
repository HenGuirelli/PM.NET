using PM.Managers;
using System.Collections.Concurrent;

namespace PM.Collections.Internals
{
    internal class LimitedDictItem<TKey, TValue>
    {
        public TValue Value { get; set; }
        public TKey Key { get; set; }
        public DateTime AddedDatetime { get; } = DateTime.Now;
        public DateTime LastGetDatetime { get; private set; } = DateTime.Now;

        internal void UpdateLastGetTime(List<LimitedDictItem<TKey, TValue>> items)
        {
            LastGetDatetime = DateTime.Now;
            items.Remove(this);
            items.Add(this);
        }
    }

    internal class FileHandlerTimedCollection
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

        public bool TryAdd(string key, FileHandlerItem value)
        {
            if (Count >= Capacity)
            {
                throw new ApplicationException("Capacity limit reached");
            }
            var dictItem = new LimitedDictItem<string, FileHandlerItem> { Value = value };
            _orderedFileHandlers.Add(dictItem);
            return _fileHandlersByFilename.TryAdd(key, dictItem);
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

        public IEnumerable<LimitedDictItem<string, FileHandlerItem>> CleanOldValues(int qtyToClean)
        {
            var removedItems = new List<LimitedDictItem<string, FileHandlerItem>>();
            for (int i = 0; i < qtyToClean; i++)
            {
                LimitedDictItem<string, FileHandlerItem> itemToRemove = _orderedFileHandlers[i];
                _orderedFileHandlers.Remove(itemToRemove);
                _fileHandlersByFilename.Remove(itemToRemove.Key, out var _);

                removedItems.Add(itemToRemove);
            }
            return removedItems;

        }
    }
}
