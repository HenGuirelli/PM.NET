using PM.CastleHelpers;
using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;
using System.Collections;
using System.Collections.Concurrent;

namespace PM.Collections
{
    public class PmList<T> : IList<T>
        where T : class, new()
    {
        private readonly IPersistentFactory _persistentFactory = new PersistentFactory();
        private readonly static PointersToPersistentObjects _pointersToPersistentObjects = new();
        private readonly ConcurrentDictionary<ulong, T> _cacheItems = new();

        public T this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        public int Count => _size;
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
                if (value < _size)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value != _items.Length && value > 0)
                {
                    _items = CreateNewInternalArray(Filepath, value);
                }
            }
        }

        public string Filepath { get; }

        internal PmPrimitiveArray<ulong> _items;
        internal int _size;
        internal int _version;
        public const int DefaultCapacity = 4;

        public PmList(string filepath, int initialCapacity = DefaultCapacity)
        {
            Filepath = filepath;
            _items = CreateNewInternalArray(filepath, initialCapacity);
            for(int i = 0; i < _items.Length; i++)
            {
                if (_items[i] != 0) _size++;
            }
        }

        private PmPrimitiveArray<ulong> CreateNewInternalArray(string filepath, int length)
        {
            var pm = PmFactory.CreatePm(
                new PmMemoryMappedFileConfig(
                    filepath,
                    sizeof(ulong) * length));

            return PmPrimitiveArray.CreateNewArray<ulong>(pm, length);
        }

        [Obsolete("Use AddPersistent(T) instead")]
        public void Add(T item)
        {
            AddPersistent(item);
        }

        public T Set(int index, T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (index > _size) throw new ArgumentOutOfRangeException(nameof(item));

            var pointer = _pointersToPersistentObjects.GetNext();
            var pmFile = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString());
            var obj = _persistentFactory.CreateRootObjectByObject(item, pmFile);

            _items[index] = pointer;
            _cacheItems[pointer] = (T)obj;
            return (T)obj;
        }

        private T Get(int index)
        {
            if (index >= _size) throw new IndexOutOfRangeException();

            var pointer = _items[index];
            if (_cacheItems.TryGetValue(pointer, out var result))
            {
                return result;
            }
            var pmFile = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString());
            var obj = _persistentFactory.CreateRootObject<T>(pmFile);

            _cacheItems.TryAdd(pointer, obj);

            return obj;
        }

        public T AddPersistent(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            if (CastleManager.TryGetInterceptor(item, out _))
            {
                throw new ArgumentException($"{nameof(item)} argument cannot be persistent object");
            }

            var pointer = _pointersToPersistentObjects.GetNext();
            var obj = _persistentFactory.CreateRootObjectByObject(
                item,
                Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString()));

            _items[_size++] = pointer;
            _cacheItems[pointer] = (T)obj;
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
            _items.Clear();
            _size = 0;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < _size; i++)
                    if (_items[i] == null)
                        return true;
                return false;
            }
            else
            {
                for (int i = 0; i < _size; i++)
                {
                    if (Get(i) == item) return true;
                }
                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < _size; i++)
            {
                array[i] = Get(i);
            }
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < _size; i++)
            {
                var internalItem = Get(i);
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
            if (index > _size)
            {
                throw new IndexOutOfRangeException();
            }
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            if (CastleManager.TryGetInterceptor(item, out _))
            {
                throw new ArgumentException($"{nameof(item)} argument cannot be persistent object");
            }

            var pointer = _pointersToPersistentObjects.GetNext();
            var obj = _persistentFactory.CreateRootObjectByObject(
                item,
                Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString()));

            _items[_size++] = pointer;
            _cacheItems[pointer] = (T)obj;
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
            if (index >= _size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (index < _size)
            {
                for (int i = index; i < _size - 1; i++)
                {
                    _items[i] = _items[i + 1];
                }
            }
            _items[_size - 1] = default;
            _size--;
            var pm = PmFactory.CreatePm(
                new PmMemoryMappedFileConfig(
                    Path.Combine(PmGlobalConfiguration.PmInternalsFolder, _items[index].ToString())));
            pm.DeleteFile();
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
        private int _version;
        private T _current;
        private readonly IPersistentFactory _persistentFactory = new PersistentFactory();

        internal Enumerator(PmList<T> list)
        {
            _list = list;
            _index = 0;
            _version = list._version;
            _current = default;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            PmList<T> localList = _list;

            if (_version == localList._version && _index < localList._size)
            {
                _current = localList[_index];
                _index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            if (_version != _list._version)
            {
                throw new ApplicationException("Version updated while a interation");
            }

            _index = _list._size + 1;
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
                if (_index == 0 || _index == _list._size + 1)
                {
                    throw new ApplicationException("Index out of bounds");
                }
                return Current;
            }
        }

        void System.Collections.IEnumerator.Reset()
        {
            if (_version != _list._version)
            {
                throw new ApplicationException("Version updated while a interation");
            }

            _index = 0;
            _current = default;
        }
    }
}