namespace PM.Collections
{
    public interface IHashMap<TKey, TValue> : IEnumerable<PmKeyValuePair<TKey, TValue>>
    {
        void Put(TKey key, TValue value);
    }

    public interface ILinkedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// Adds a new node at the beginning of the list.
        /// </summary>
        /// <param name="value">The value to be added.</param>
        void Prepend(T value);

        /// <summary>
        /// Adds a new node at the end of the list.
        /// </summary>
        /// <param name="value">The value to be added.</param>
        void Append(ref T value);

        /// <summary>
        /// Inserts a new node at a specific position in the list.
        /// </summary>
        /// <param name="value">The value to be added.</param>
        /// <param name="index">The position where the new node will be inserted.</param>
        void InsertAt(T value, int index);

        /// <summary>
        /// Removes the node at the beginning of the list.
        /// </summary>
        void RemoveFirst();

        /// <summary>
        /// Removes the node at the end of the list.
        /// </summary>
        void RemoveLast();

        /// <summary>
        /// Removes the node at a specific position in the list.
        /// </summary>
        /// <param name="index">The position of the node to be removed.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Removes the first node that contains a specific value.
        /// </summary>
        /// <param name="value">The value of the node to be removed.</param>
        void RemoveByValue(T value);

        /// <summary>
        /// Gets the value of the node at a specific position.
        /// </summary>
        /// <param name="index">The position of the node.</param>
        /// <returns>The value of the node at the specified position.</returns>
        T GetValueAt(int index);

        /// <summary>
        /// Finds the index of the first node that contains a specific value.
        /// </summary>
        /// <param name="value">The value to be searched for.</param>
        /// <returns>The index of the node, or -1 if not found.</returns>
        int Find(T value);

        /// <summary>
        /// Returns the number of elements in the list.
        /// </summary>
        /// <returns>The number of elements in the list.</returns>
        int Size();

        /// <summary>
        /// Checks if the list is empty.
        /// </summary>
        /// <returns>True if the list is empty; otherwise, false.</returns>
        bool IsEmpty();
    }
}
