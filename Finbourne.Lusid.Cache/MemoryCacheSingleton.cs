﻿using Ardalis.GuardClauses;
using Finbourne.Lusid.Cache.CacheStrategy;
using Finbourne.Lusid.Cache.Extensions;
using Finbourne.Lusid.Cache.Models.Exceptions;
using NLog;
using System.Collections.Concurrent;

namespace Finbourne.Lusid.Cache
{
    public sealed class MemoryCacheSingleton<T> : ICache<T>
    {
        private static readonly object _locker = new();
        private static readonly MemoryCacheSingleton<T> instance = new();
        private ConcurrentDictionary<string, CacheObject<T>> _cache;
        private ILogger _logger;
        private int _usageThresholdCount;

        public event EventHandler<CacheObjectRemovedEventArgs> CacheObjectRemovedEvent;

        static MemoryCacheSingleton() { }
        MemoryCacheSingleton() { }

        public static MemoryCacheSingleton<T> Instance
        {
            get
            {
                return instance;
            }
        }

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

        public bool TryGetItem(string key, out T item)
        {
            Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
            Guard.Against.NullOrEmpty(key, message: "The cache key must have a value");
            _logger.Info($"Retrieving cached item for key '{key}'");
            bool keyFound = _cache.TryGetValue(key, out var cacheObject);
            item = GetCachedItemIfKeyIsFoundElseDefault(keyFound, cacheObject);
            return keyFound;
        }

        public int ItemCount
        {
            get
            {
                Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
                return _cache.Count;
            }
        }

        public bool KeyExists(string key)
        {
            Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
            Guard.Against.NullOrEmpty(key, message: "The cache key must have a value");
            return _cache.ContainsKey(key);
        }

        public int UsageCount(string key)
        {
            Guard.Against.Null(_cache, message: "The cache object has not been setup.  Please run Initialise method");
            Guard.Against.NullOrEmpty(key, message: "The cache key must have a value");
            bool keyFound = _cache.TryGetValue(key, out var cacheObject);
            return keyFound ? cacheObject.UsageCount : -1;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var item in _cache.Values)
            {
                item.Dispose();
            }
        }

        private T GetCachedItemIfKeyIsFoundElseDefault(bool keyFound, CacheObject<T> cacheObject)
        {
            T item = cacheObject.GetItem ?? default;
            _logger.Info(keyFound ? $"Object of type {item?.GetType()} retrieved from cache" : "Key not found");
            return item;
        }

        private static CacheObject<T> CreateCacheObjectToStore(string key, T item)
        {
            return new CacheObject<T>
            {
                Key = key,
                AddItem = item
            };
        }

        private bool MakeSureThatCacheKeyIsUniqueWriteErrorIfNot(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _logger.Warn($"Item not added since the key '{key}' already exists");
                return false;
            }
            return true;
        }

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

        private KeyValuePair<string, CacheObject<T>> GetLeastUsedItemFromCache()
        {
            return _cache.Aggregate((min, next) => next.Value.UsageCount < min.Value.UsageCount ? next : min);
        }

        private void OnCacheItemRemoved(string key, int usageCount)
        {
            var args = new CacheObjectRemovedEventArgs
            {
                Key = key,
                ThresholdUsageCount = usageCount
            };
            CacheObjectRemovedEvent.Raise(this, args);
        }

    }
}
