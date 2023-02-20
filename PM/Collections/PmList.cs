using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;
using System.Collections;

namespace PM.Collections
{
    public class PmList<T> : IList<T>
        where T : class, new()
    {
        private readonly IPersistentFactory _persistentFactory = new PersistentFactory();
        private static readonly PointersToPersistentObjects _pointersToPersistentObjects = new();

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

                if (value != _items.Length)
                {
                    if (value > 0)
                    {
                        if (_size > 0)
                        {
                            var newItems = new PmStringArray(Filepath, value);
                            _items = newItems;
                        }
                    }
                }
            }
        }

        public string Filepath { get; }

        private PmStringArray _items;
        private int _size;
        public const int DefaultCapacity = 4;

        public PmList(string filepath, int initialCapacity = DefaultCapacity)
        {
            Filepath = filepath;
            _items = new PmStringArray(filepath, initialCapacity);
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

            var pointer = _pointersToPersistentObjects.GetNext().ToString();
            var pmFile = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer);
            var obj = _persistentFactory.CreateRootObjectByObject(item, pmFile);

            _items[index] = pointer;
            return (T)obj;
        }

        private T Get(int index)
        {
            if (index >= _size) throw new IndexOutOfRangeException();

            var pmFile = _items[index];
            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj = persistentFactory.CreateRootObject<T>(pmFile);

            return obj;
        }

        public T AddPersistent(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (_size == _items.Length) EnsureCapacity(_size + 1);

            var pointer = _pointersToPersistentObjects.GetNext().ToString();
            var obj = _persistentFactory.CreateRootObjectByObject(item, pointer);

            _items[_size++] = pointer;
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
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
