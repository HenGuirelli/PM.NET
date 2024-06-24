using PM.AutomaticManager;
using PM.AutomaticManager.Tansactions;
using PM.Core.PMemory;
using PM.FileEngine;
using System.Collections;

namespace PM.Collections
{
    public class PmLinkedListNode<T>
    {
        public virtual PmLinkedListNode<T> Next { get; set; }
        public virtual T Value { get; set; }
    }

    public class PmLinkedListEnumerator<T> : IEnumerator<T>
    {
        private readonly PmLinkedListNode<T>? _head;
        private PmLinkedListNode<T>? _currentNode;

        public T Current => _currentNode.Value;
        object IEnumerator.Current => _currentNode.Value;

        public PmLinkedListEnumerator(PmLinkedListNode<T>? head)
        {
            _head = head;
            _currentNode = null;
        }

        public bool MoveNext()
        {
            if (_head == null) return false;

            if (_currentNode == null)
            {
                _currentNode = _head;
                return true;
            }
            else if (_currentNode.Next == null)
            {
                return false;
            }
            _currentNode = _currentNode.Next;
            return true;
        }

        public void Reset()
        {
            _currentNode = _head;
        }

        public void Dispose()
        {
        }
    }

    public class PmLinkedList<T> : ILinkedList<T>
    {
        private readonly Type _nodeType;
        private readonly string _objectUserID;
        private readonly PMemoryManager _pMemoryManager;
        private readonly PAllocator _pAllocator;
        private readonly IEqualityComparer<T>? _equalityComparer;
        private readonly ObjectPropertiesInfoMapper _objectPropertiesInfoMapper;
        private PmLinkedListNode<T>? _head;
        private PmLinkedListNode<T>? _lastNode;
        private PersistentRegion? _headRegion;

        internal PmLinkedList(string objectUserID, PMemoryManager pMemoryManager, IEqualityComparer<T>? equalityComparer = null)
        {
            _nodeType = typeof(PmLinkedListNode<T>);
            _objectUserID = objectUserID;
            _pMemoryManager = pMemoryManager;
            _pAllocator = pMemoryManager.Allocator;
            _equalityComparer = equalityComparer;
            _objectPropertiesInfoMapper = new ObjectPropertiesInfoMapper(typeof(PmLinkedListNode<T>));
        }

        public void Append(ref T value)
        {
            var innerObjectFactory = new InnerObjectFactory(_pMemoryManager);
            if (_head is null)
            {
                if (!_pMemoryManager.ObjectExists(_objectUserID))
                {
                    _headRegion = _pMemoryManager.AllocRootObjectByType(_nodeType, _objectUserID);
                    _head = (PmLinkedListNode<T>)innerObjectFactory.CreateInnerObject(_headRegion, _nodeType);
                    _head.Value = value;
                    // Set proxy object to argument 'value'
                    value = _head.Value;
                    return;
                }
                else
                {
                    _headRegion = _pMemoryManager.GetRegionByObjectUserID(_objectUserID);
                    _head = (PmLinkedListNode<T>)innerObjectFactory.CreateInnerObject(_headRegion, _nodeType);
                }
            }
            if (_lastNode == null)
            {
                PmLinkedListNode<T> current = _head;
                while (current.Next != null)
                {
                    current = current.Next;
                }
                _lastNode = current;
            }

            PersistentRegion newRegion = _pAllocator.Alloc(_objectPropertiesInfoMapper.GetTypeSize());
            var newNode = (PmLinkedListNode<T>)innerObjectFactory.CreateInnerObject(newRegion, _nodeType);
            _lastNode.Next = newNode;
            _lastNode = newNode;
            newNode.Value = value;
            // Set proxy object to argument 'value'
            value = newNode.Value;
        }

        public int Find(T value)
        {
            PmLinkedListNode<T>? current = _head;
            int i = 0;
            while (current != null)
            {
                if (_equalityComparer != null)
                {
                    if (_equalityComparer.Equals(current.Value, value)) return i;
                }
                else
                {
                    if (value != null && current.Value!.Equals(value)) return i;
                }

                current = current.Next;
                i++;
            }
            return -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PmLinkedListEnumerator<T>(_head);
        }

        public T GetValueAt(int index)
        {
            if (_head is null) throw new ArgumentOutOfRangeException(nameof(index));

            PmLinkedListNode<T> current = _head;
            int i = 0;
            while (current != null)
            {
                current = current.Next;
                if (i == index) return current.Value;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        public void InsertAt(T value, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty()
        {
            throw new NotImplementedException();
        }

        public void Prepend(T value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            if (_head is null) throw new ArgumentOutOfRangeException(nameof(index));

            if (index == 0)
            {
                var newHead = _head.Next;
                _head.Transaction(_pMemoryManager, () =>
                {
                    if (newHead is null)
                    {
                        _head.Value = default;
                        _head.Next = default;
                    }
                    else
                    {
                        _head.Value = newHead.Value;
                        _head.Next = newHead.Next;
                    }
                });
            }

            PmLinkedListNode<T> nodeBeforeRemove = _head;
            PmLinkedListNode<T> nodeToRemove = _head;
            int i = 0;
            while (nodeToRemove != null)
            {
                if (i + 1 == index)
                {
                    nodeBeforeRemove = nodeToRemove;
                }

                if (i == index)
                {
                    break;
                }
                nodeToRemove = nodeToRemove.Next;
            }

            nodeBeforeRemove.Next = nodeToRemove.Next;
        }

        public void RemoveByValue(T value)
        {
            throw new NotImplementedException();
        }

        public void RemoveFirst()
        {
            throw new NotImplementedException();
        }

        public void RemoveLast()
        {
            throw new NotImplementedException();
        }

        public int Size()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
