using Ardalis.GuardClauses;
using Finbourne.Lusid.Cache.CacheStrategy;
using Finbourne.Lusid.Cache.Extensions;
using Finbourne.Lusid.Cache.Models.Exceptions;
using NLog;
using System.Collections.Concurrent;

namespace Finbourne.Lusid.Cache
{
    public sealed class MemoryCache<T> : ICache<T>
    {
        private static readonly object _locker = new();
        private static readonly MemoryCache<T> instance = new();
        private ConcurrentDictionary<string, CacheObject<T>> _cache;
        private ILogger _logger;
        private int _usageThresholdCount;

        static MemoryCache() { }
        MemoryCache() { }

        /// <summary>
        /// Return the singleton instance
        /// </summary>
        public static MemoryCache<T> Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Event fired when an item is removed from cache when threshold is crossed
        /// </summary>
        public event EventHandler<CacheObjectRemovedEventArgs> CacheObjectRemovedEvent;

        /// <summary>
        /// Setup the singleton memory cache.  This should be the 1st call made to this class
        /// </summary>
        /// <param name="usageThresholdCount">The number of items that can be stored in the cache</param>
        /// <param name="logger">The logger to write messages to</param>
        public void Initialise(int usageThresholdCount, ILogger logger)
        {
            lock (_locker)
            {
                if (_cache != null) return;
                _cache = new ConcurrentDictionary<string, CacheObject<T>>();
            }
            _usageThresholdCount = usageThresholdCount;
            _logger = logger;
        }

        /// <summary>
        /// Add an item to the cache
        /// </summary>
        /// <param name="key">The item key in the cache</param>
        /// <param name="item">The item to add</param>
        /// <returns>true if added, false if not</returns>
        public bool TryAddItem(string key, T item)
        {
            Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
            Guard.Against.NullOrEmpty(key, message: "The cache key must have a value");
            Guard.Against.NegativeOrZero(_usageThresholdCount, message: $"Threshold usage count must be greater than 0.  Currently set to {_usageThresholdCount}");
            if (!MakeSureThatCacheKeyIsUniqueWriteErrorIfNot(key))
            {
                return true;
            }

            var cacheObject = CreateCacheObjectToStore(key, item);
            CheckThresholdLimitAndSeeIfWeNeedToRemoveAnItem(key);
            return _cache.TryAdd(key, cacheObject);
        }

        /// <summary>
        /// Retrieve an item from the cache
        /// </summary>
        /// <param name="key">The key used in the cache</param>
        /// <param name="item">The retrieve item</param>
        /// <returns>True/False depending on if the item is found or not</returns>
        public bool TryGetItem(string key, out T item)
        {
            Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
            Guard.Against.NullOrEmpty(key, message: "The cache key must have a value");
            _logger.Info($"Retrieving cached item for key '{key}'");
            bool keyFound = _cache.TryGetValue(key, out var cacheObject);
            item = GetCachedItemIfKeyIsFoundElseDefault(keyFound, cacheObject);
            return keyFound;
        }

        /// <summary>
        /// Check to see if the key exists in the cache
        /// </summary>
        /// <param name="key">The key used in the cache</param>
        /// <returns>True if key exist in the cache and false if it does not</returns>
        public bool KeyExists(string key)
        {
            Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
            Guard.Against.NullOrEmpty(key, message: "The cache key must have a value");
            return _cache.ContainsKey(key);
        }

        /// <summary>
        /// Return how many times an item has been retrieved from the cache
        /// </summary>
        /// <param name="key">The items key in the cache</param>
        /// <returns>The number of times the item has been retrieved</returns>
        public int UsageCount(string key)
        {
            Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
            Guard.Against.NullOrEmpty(key, message: "The cache key must have a value");
            bool keyFound = _cache.TryGetValue(key, out var cacheObject);
            return keyFound ? cacheObject.UsageCount : -1;
        }

        /// <summary>
        /// Return the number of cached items
        /// </summary>
        public int ItemCount
        {
            get
            {
                Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
                return _cache.Count;
            }
        }

        /// <summary>
        /// Dispose the cached objects
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var item in _cache.Values)
            {
                item.Dispose();
            }
        }

        #region private methods

        /// <summary>
        /// From the cache object, return the item.  If the key is not found, then return default
        /// </summary>
        /// <param name="keyFound">Has the key been located</param>
        /// <param name="cacheObject">The returned cache object</param>
        /// <returns></returns>
        private T GetCachedItemIfKeyIsFoundElseDefault(bool keyFound, CacheObject<T> cacheObject)
        {
            T item = (cacheObject == null) ? default : cacheObject.GetItem ?? default;
            _logger.Info(keyFound ? $"Object of type {item?.GetType()} retrieved from cache" : "Key not found");
            return item;
        }

        /// <summary>
        /// Create a cache object used to store in the cache
        /// </summary>
        /// <param name="key">The key used in the cache</param>
        /// <param name="item">The item to cache</param>
        /// <returns></returns>
        private static CacheObject<T> CreateCacheObjectToStore(string key, T item)
        {
            return new CacheObject<T>
            {
                Key = key,
                AddItem = item
            };
        }

        /// <summary>
        /// Validate key to make sure it is unique
        /// </summary>
        /// <param name="key">The key to find</param>
        /// <returns>True/False depending on if the key is found</returns>
        private bool MakeSureThatCacheKeyIsUniqueWriteErrorIfNot(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _logger.Warn($"Item not added since the key '{key}' already exists");
                return false;
            }
            return true;
        }

        /// <summary>
        /// If we reached the threshold limit, remove the least used on, add to the cache and raise event
        /// </summary>
        /// <param name="key">The key used in the cache</param>
        private void CheckThresholdLimitAndSeeIfWeNeedToRemoveAnItem(string key)
        {
            if (_cache.Count == _usageThresholdCount)
            {
                var leastUsedObject = GetLeastUsedItemFromCache();
                _cache.TryRemove(leastUsedObject);
                _logger.Info($"Replaced cached object {leastUsedObject.Key} with {key}");
                OnCacheItemRemoved(leastUsedObject.Key, leastUsedObject.Value.UsageCount);
            }
        }

        /// <summary>
        /// Find the least used cache item
        /// </summary>
        /// <returns>The least used item</returns>
        private KeyValuePair<string, CacheObject<T>> GetLeastUsedItemFromCache()
        {
            return _cache.Aggregate((min, next) => next.Value.UsageCount < min.Value.UsageCount ? next : min);
        }

        /// <summary>
        /// Raise event when an item has been removed from cache
        /// </summary>
        /// <param name="key">The key used in the cache</param>
        /// <param name="usageCount">How many times has the cached object been used</param>
        private void OnCacheItemRemoved(string key, int usageCount)
        {
            var args = new CacheObjectRemovedEventArgs
            {
                Key = key,
                ThresholdUsageCount = usageCount
            };
            CacheObjectRemovedEvent.Raise(this, args);
        }

        #endregion private methods

    }
}
