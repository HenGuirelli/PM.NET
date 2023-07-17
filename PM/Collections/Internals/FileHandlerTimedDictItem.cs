namespace PM.Collections.Internals
{
    public class FileHandlerTimedDictItem<TKey, TValue>
    {
        public TValue Value { get; }
        public TKey Key { get; }
        public DateTime AddedDatetime { get; } = DateTime.Now;
        public DateTime LastGetDatetime { get; private set; } = DateTime.Now;

        public FileHandlerTimedDictItem(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        internal void UpdateLastGetTime(List<FileHandlerTimedDictItem<TKey, TValue>> items)
        {
            LastGetDatetime = DateTime.Now;
            items.Remove(this);
            items.Add(this);
        }
    }
}
