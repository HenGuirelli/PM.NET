using PM.CastleHelpers;
using PM.Configs;
using PM.Core;
using PM.Managers;
using System.Collections;
using System.Collections.Concurrent;

namespace PM.Collections
{
    public class PmList<T> : IList<T>, IPmList, ICustomPmClass
        where T : class, new()
    {
        private readonly IPersistentFactory _persistentFactory = new PersistentFactory();
        private readonly static PointersToPersistentObjects _pointersToPersistentObjects = new();
        private readonly ConcurrentDictionary<ulong, object> _cache = new();

        public T this[int index]
        {
            get => Get(index, false);
            set => Set(index, value);
        }

        public int Count => ListCount - 1;
        public bool IsReadOnly => false;

        // Gets and sets the capacity of this list.  The capacity is the size of
        // the internal array used to hold items.  When set, the internal 
        // array of the list is reallocated to the given capacity.
        // 
        public int Capacity
        {
            get
            {
                return _items.Length;
            }
            set
            {
                if (value < ListCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value >= _items.Length)
                {
                    _items.Resize(value);
                }
            }
        }

        public string Filepath { get; }

        internal PmPrimitiveArray<ulong> _items;
        internal int _listCount;
        internal int ListCount
        {
            get => _listCount;
            set
            {
                _items[0] = (ulong)value;
                _listCount = value;
            }
        }

        public ulong PmPointer { get; }

        public const int DefaultCapacity = 4;

        public PmList(string symbolicLink, int initialCapacity = DefaultCapacity)
        {
            if (initialCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            Filepath = symbolicLink;
            _items = CreateOrLoadInternalArray(symbolicLink, initialCapacity);
            _listCount = (int)_items[0];

            // Increment 1 because the fist element always exists (array size)
            if (_listCount == 0) _listCount++;
            PmPointer = PmFileSystem.GetPointerFromSymbolicLink(symbolicLink);
        }

        internal PmList(string pmFile, ulong pointer)
        {
            Filepath = pmFile;
            _items = InternalLoadPmFileList(pmFile);
            _listCount = (int)_items[0];
            // Increment 1 because the fist element always exists(array size)
            if (_listCount == 0) _listCount++;
            PmPointer = pointer;
        }

        private PmPrimitiveArray<ulong> CreateOrLoadInternalArray(string symbolicLink, int capacity)
        {
            if (!PmFileSystem.FileExists(symbolicLink))
            {
                return CreateNewPmFileList(symbolicLink, capacity);
            }
            else
            {
                return LoadPmFileList(symbolicLink);
            }
        }

        private PmPrimitiveArray<ulong> InternalLoadPmFileList(string targetFilename)
        {
            var size = PmFileSystem.GetFileSize(targetFilename);
            var capacity = (int)(size / sizeof(ulong));
            return CollectionsPmFactory.CreateULongArray(targetFilename, capacity);
        }

        private PmPrimitiveArray<ulong> LoadPmFileList(string symbolicLink)
        {
            var size = PmFileSystem.GetFileSize(symbolicLink);
            var targetFilename = PmFileSystem.GetTargetOfSymbolicLink(symbolicLink);
            var capacity = (int)(size / sizeof(ulong));
            return CollectionsPmFactory.CreateULongArray(targetFilename, capacity);
        }

        private static PmPrimitiveArray<ulong> CreateNewPmFileList(string symbolicLink, int capacity)
        {
            var pointer = _pointersToPersistentObjects.GetNext().ToString();
            string targetFilename = PmFileSystem.CreateSymbolicLinkInInternalsFolder(
                symbolicLink,
                pointer + ".root");
            return CollectionsPmFactory.CreateULongArray(targetFilename, capacity + 1);
        }

        [Obsolete("Use AddPersistent(T) instead")]
        public void Add(T item)
        {
            AddPersistent(item);
        }

        public T Set(int index, T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (index > Count) throw new ArgumentOutOfRangeException(nameof(item));

            // Increment because first element is the Size
            index++;
            var pointer = _pointersToPersistentObjects.GetNext();
            var obj = _persistentFactory.CreateInternalObjectByObject(item, pointer);

            _items[index] = pointer;
            _cache[pointer] = obj;
            return (T)obj;
        }

        private T Get(int index, bool ignoreFirstItem)
        {
            if (ignoreFirstItem)
            {
                if (index > Count) throw new IndexOutOfRangeException();
            }
            else
            {
                if (index >= Count) throw new IndexOutOfRangeException();
            }

            // Increment because first element is the Size
            index++;

            var pointer = _items[index];

            if (pointer == 0)
            {
                throw new ApplicationException("pointer invalid: 0");
            }

            if (_cache.TryGetValue(pointer, out var result)) return (T)result;

            var pmFile = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString() + ".pm");
            var obj = (T)_persistentFactory.LoadFromFile(typeof(T), pmFile, pointer);
            _cache[pointer] = obj;
            return obj;
        }

        public T AddPersistent(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (ListCount == _items.Length) EnsureCapacity(ListCount + 1);
            if (CastleManager.TryGetInterceptor(item, out _))
            {
                throw new ArgumentException($"{nameof(item)} argument cannot be persistent object");
            }

            var pointer = _pointersToPersistentObjects.GetNext();
            var obj = _persistentFactory.CreateInternalObjectByObject(item, pointer);

            _items[ListCount++] = pointer;
            _cache[pointer] = obj;
            _items.Flush();
            return (T)obj;
        }


        // Ensures that the capacity of this list is at least the given minimum
        // value. If the currect capacity of the list is less than min, the
        // capacity is increased to twice the current capacity or to min,
        // whichever is larger.
        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                _items[i] = 0;
            }
            ListCount = 1;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < ListCount; i++)
                    if (_items[i] == null)
                        return true;
                return false;
            }
            else
            {
                for (int i = 0; i < ListCount; i++)
                {
                    if (Get(i, false) == item) return true;
                }
                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = Get(i, ignoreFirstItem: true);
            }
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                var internalItem = Get(i, true);
                if (item == internalItem) return i;
            }
            return -1;
        }

        [Obsolete("Use InsertPersistent() instead")]
        public void Insert(int index, T item)
        {
            InsertPersistent(index, item);
        }

        public T InsertPersistent(int index, T item)
        {
            // Note that insertions at the end are legal.
            if (index > ListCount)
            {
                throw new IndexOutOfRangeException();
            }
            if (ListCount == _items.Length) EnsureCapacity(ListCount + 1);
            if (CastleManager.TryGetInterceptor(item, out _))
            {
                throw new ArgumentException($"{nameof(item)} argument cannot be persistent object");
            }

            var pointer = _pointersToPersistentObjects.GetNext();
            var obj = _persistentFactory.CreateInternalObjectByObject(item, pointer);

            _items[ListCount++] = pointer;
            _items.Flush();
            return (T)obj;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (index >= ListCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (index < ListCount)
            {
                for (int i = index; i < ListCount - 1; i++)
                {
                    _items[i] = _items[i + 1];
                }
            }
            _items[ListCount - 1] = default;
            ListCount--;
            //var pm = PmFactory.CreatePm(
            //        Path.Combine(PmGlobalConfiguration.PmInternalsFolder, _items[index].ToString()));
            //pm.DeleteFile();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(this);
        }
    }

    [Serializable]
    public struct Enumerator<T> : IEnumerator<T>, System.Collections.IEnumerator
        where T : class, new()
    {
        private PmList<T> _list;
        private int _index;
        private T _current;
        private readonly IPersistentFactory _persistentFactory = new PersistentFactory();

        internal Enumerator(PmList<T> list)
        {
            _list = list;
            _index = 0;
            _current = default;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            PmList<T> localList = _list;

            if (_index < localList.Count)
            {
                _current = localList[_index];
                _index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            _index = _list.ListCount + 1;
            _current = default;
            return false;
        }

        public T Current
        {
            get
            {
                return _current;
            }
        }

        Object System.Collections.IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _list.ListCount + 1)
                {
                    throw new ApplicationException("Index out of bounds");
                }
                return Current;
            }
        }

        void System.Collections.IEnumerator.Reset()
        {
            _index = 0;
            _current = default;
        }
    }
}