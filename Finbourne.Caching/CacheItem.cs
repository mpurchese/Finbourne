namespace Finbourne.Caching
{
    public class CacheItem<TKey, TValue> where TKey: notnull
    {
        public TKey Key { get; }
        public TValue Value { get; set; }

        public CacheItem(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
