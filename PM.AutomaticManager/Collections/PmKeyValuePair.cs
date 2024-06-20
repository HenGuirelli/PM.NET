namespace PM.Collections
{
    public class PmKeyValuePair<TKey, TValue>
    {
        public virtual TKey Key { get; set; }
        public virtual TValue Value { get; set; }

        public PmKeyValuePair() { }

        public PmKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
