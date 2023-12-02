using PM.Core;
using PM.Managers;
using Serilog;
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
        
        internal readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        
        private readonly ConcurrentDictionary<string, FileHandlerTimedDictItem<string, FileHandlerItem>>
            _fileHandlersByFilename = new();
        private readonly List<FileHandlerTimedDictItem<string, FileHandlerItem>>
            _orderedFileHandlers = new();

        public FileHandlerTimedCollection(int capacity)
        {
            Capacity = capacity;
        }

        public bool TryGetValue(string key, out FileHandlerItem value)
        {
            try
            {
                _lock.EnterReadLock();
                if (_fileHandlersByFilename.TryGetValue(key, out var dictItem))
                {
                    Log.Verbose("{key} get on TryGetValue sucessfully", key);
                    value = dictItem.Value;
                    dictItem.UpdateLastGetTime(_orderedFileHandlers);
                    return true;
                }
                Log.Verbose("{key} not found on TryGetValue", key);
                value = default;
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Add(string key, FileHandlerItem value)
        {
            try
            {
                _lock.EnterWriteLock();
                if (Count >= Capacity)
                {
                    throw new CollectionLimitReachedException(Capacity);
                }
                var dictItem = new FileHandlerTimedDictItem<string, FileHandlerItem>(key, value);
                if (_fileHandlersByFilename.TryAdd(key, dictItem))
                {
                    _orderedFileHandlers.Add(dictItem);
                    Log.Verbose("{class} Add {key}", nameof(FileHandlerTimedCollection), key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryRemove(string key, out FileHandlerItem value)
        {
            try
            {
                _lock.EnterWriteLock();
                if (_fileHandlersByFilename.TryRemove(key, out var dictItem))
                {
                    value = dictItem.Value;

                    return true;
                }
                value = default;
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<MemoryMappedFileBasedStream> CleanOldValues(int qtyToClean)
        {
            try
            {
                _lock.EnterWriteLock();
                var removedItemsFileBasedStream = new List<MemoryMappedFileBasedStream>();
                var removedItems = new List<FileHandlerTimedDictItem<string, FileHandlerItem>>();
                for (int i = 0; i < qtyToClean; i++)
                {
                    FileHandlerTimedDictItem<string, FileHandlerItem> itemToRemove = _orderedFileHandlers[i];
                    if (itemToRemove != null)
                    {
                        _orderedFileHandlers.Remove(itemToRemove);
                        _fileHandlersByFilename.Remove(itemToRemove.Key, out var _);
                        removedItemsFileBasedStream.Add(itemToRemove.Value.FileBasedStream);
                        removedItems.Add(itemToRemove);

                        Log.Verbose("{key} removed on FileHandlerTimedCollection.CleanOldValues",
                            itemToRemove.Key);
                    }
                }
                foreach (var item in removedItems)
                {
                    _orderedFileHandlers.Remove(item);
                }
                return removedItemsFileBasedStream;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
