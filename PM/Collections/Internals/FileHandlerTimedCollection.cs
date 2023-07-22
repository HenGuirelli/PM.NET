using PM.Core;
using PM.Managers;
using System.Collections.Concurrent;

namespace PM.Collections.Internals
{
    public class CollectionLimitReachedException : ApplicationException
    {
        public CollectionLimitReachedException(int limit)
            : base($"Capacity limit of {limit} reached")
        {
        }
    }

    public class FileHandlerTimedCollection
    {
        public int Capacity { get; set; }
        public int Count => _fileHandlersByFilename.Count;
        private readonly ConcurrentDictionary<string, FileHandlerTimedDictItem<string, FileHandlerItem>>
            _fileHandlersByFilename = new();
        private readonly List<FileHandlerTimedDictItem<string, FileHandlerItem>>
            _orderedFileHandlers = new();

        public FileHandlerTimedCollection(int capacity)
        {
            Capacity = capacity;
        }

        public void Add(string key, FileHandlerItem value)
        {
            if (Count >= Capacity)
            {
                throw new CollectionLimitReachedException(Capacity);
            }
            var dictItem = new FileHandlerTimedDictItem<string, FileHandlerItem>(key, value);
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
                FileHandlerTimedDictItem<string, FileHandlerItem> itemToRemove = _orderedFileHandlers[i];
                if (itemToRemove != null)
                {
                    _orderedFileHandlers.Remove(itemToRemove);
                    _fileHandlersByFilename.Remove(itemToRemove.Key, out var _);
                }

                removedItems.Add(itemToRemove.Value.FileBasedStream);
            }
            return removedItems;
        }
    }
}
