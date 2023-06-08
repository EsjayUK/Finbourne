namespace Finbourne.Lusid.Cache.CacheStrategy
{
    /// <summary>
    /// Strategy object that can be implemented to create different types of caches.  Currently only 
    /// MemoryCache is created but this interface can be used to create other caches such as persistence 
    /// cache
    /// </summary>
    /// <typeparam name="T">The type of object to be cached</typeparam>
    public class CacheObject<T> : IDisposable
    {
        private int _usageCount;
        private T _cachedItem;

        #region Model Attributes

        /// <summary>
        /// The key to used in the cache
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Return how many times the cached object has been used
        /// </summary>
        public int UsageCount => _usageCount;

        /// <summary>
        /// Retrieve an object and increment usage
        /// </summary>
        public T GetItem
        {
            get 
            {  
                Interlocked.Increment(ref _usageCount);
                return _cachedItem; 
            }
        }

        /// <summary>
        /// Add an item to the cache
        /// </summary>            
        public T AddItem
        {
            set
            {
                _cachedItem = value;
            }
        }

        #endregion Model Attributes

        /// <summary>
        /// If the object is able to be disposed, then dispose it
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (_cachedItem is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
