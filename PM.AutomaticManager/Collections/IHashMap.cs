namespace PM.Collections
{
    public interface IHashMap<TKey, TValue> : IEnumerable<PmKeyValuePair<TKey, TValue>>
    {
        void Put(TKey key, TValue value);
        TValue? Get(TKey key);
    }
}
