namespace Finbourne.Caching
{
    // TODO: What is the use case for this?  Does the client need to know which item was evicted, or just that the cache has reached capacity?
    public delegate void CacheItemEvicted();

    public class CacheManager<TKey, TValue> where TKey : notnull
    {
        private readonly int? capacity;

        private readonly object lockObj = new();
        private readonly Dictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>> cache = new();
        private readonly LinkedList<CacheItem<TKey, TValue>> lastUsedList = new();

        /// <summary>
        /// Event raised when a cache item is evicted
        /// </summary>
        public event CacheItemEvicted? CacheItemEvicted;

        /// <summary>
        /// Create a CacheManager instance with unlimited capacity
        /// </summary>
        public CacheManager()
        {
        }

        /// <summary>
        /// Create a CacheManager instance with a defined capacity
        /// </summary>
        /// <param name="capacity">The maximum number of items in the cache.  If this limit is reached, then the least recently used item will be removed.</param>
        public CacheManager(int capacity)
        {
            this.capacity = capacity;
        }

        /// <summary>
        /// Add an item to the cache, or update existing item.  If cache reaches capacity, then the least recently used item will be removed.
        /// </summary>
        /// <param name="key">The key to add or update</param>
        /// <param name="value">The value</param>
        public void AddOrUpdate(TKey key, TValue value)
        {
            lock (lockObj)
            {
                if (cache.TryGetValue(key, out var node))
                {
                    // Update existing item
                    node.Value.Value = value;
                    this.lastUsedList.Remove(node);
                    this.lastUsedList.AddLast(node);                    
                }
                else
                {
                    // Make space if needed before adding new item
                    if (this.cache.Count >= this.capacity)
                    {
                        this.RemoveLeastRecentlyUsed();
                    }

                    var item = new CacheItem<TKey, TValue>(key, value);
                    node = this.lastUsedList.AddLast(item);
                    this.cache.Add(key, node);
                }
            }
        }

        /// <summary>
        /// Gets the vaue associated with a key, if present
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="value">The value, or default</param>
        /// <returns>True if the key was found</returns>
        public bool TryGetValue(TKey key, out TValue? value)
        {
            lock (lockObj)
            {
                if (cache.TryGetValue(key, out var node))
                {
                    this.lastUsedList.Remove(node);
                    this.lastUsedList.AddLast(node);

                    value = node.Value.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Removes a key/value pair from the cache
        /// </summary>
        /// <param name="key">The key to remove</param>
        public void Remove(TKey key)
        {
            lock (lockObj)
            {
                if (this.cache.TryGetValue(key, out var node))
                {
                    this.cache.Remove(key);
                    this.lastUsedList.Remove(node);
                }
            }
        }

        private void RemoveLeastRecentlyUsed()
        {
            var first = this.lastUsedList.First;
            if (first != null)
            {
                this.cache.Remove(first.Value.Key);
                this.lastUsedList.RemoveFirst();
                this.CacheItemEvicted?.Invoke();
            }
        }
    }
}