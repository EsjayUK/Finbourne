using Finbourne.Lusid.Cache.Models.Exceptions;

namespace Finbourne.Lusid.Cache.CacheStrategy
{
    public interface ICache<T> : IDisposable
    {
        /// <summary>
        /// Event fired when an item is removed from cache when threshold is crossed
        /// </summary>
        public event EventHandler<CacheObjectRemovedEventArgs> CacheObjectRemovedEvent;

        /// <summary>
        /// Add an item to the cache
        /// </summary>
        /// <param name="key">The item key in the cache</param>
        /// <param name="item">The item to add</param>
        /// <returns>true if added, false if not</returns>
        public bool TryAddItem(string key, T item);

        /// <summary>
        /// Retrieve an item from the cache
        /// </summary>
        /// <param name="key">The key used in the cache</param>
        /// <param name="item">The retrieve item</param>
        /// <returns>True/False depending on if the item is found or not</returns>
        public bool TryGetItem(string key, out T item);

        /// <summary>
        /// Check to see if the key exists in the cache
        /// </summary>
        /// <param name="key">The key used in the cache</param>
        /// <returns>True if key exist in the cache and false if it does not</returns>
        public bool KeyExists(string key);

        /// <summary>
        /// Return how many times an item has been retrieved from the cache
        /// </summary>
        /// <param name="key">The items key in the cache</param>
        /// <returns>The number of times the item has been retrieved</returns>
        public int UsageCount(string key);

        /// <summary>
        /// Return the number of cached items
        /// </summary>
        public int ItemCount { get; }
    }
}
