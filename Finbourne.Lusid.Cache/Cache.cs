//using Finbourne.Lusid.Cache.CacheStrategy;
//using NLog;

//namespace Finbourne.Lusid.Cache
//{
//    public class Cache : IDisposable
//    {
//        private readonly ICache<object> _cache;
//        private readonly ILogger _logger;
//        private readonly uint _itemThresholdCount;

//        public Cache(uint itemThresholdCount, ILogger logger) 
//        {
//            _cache = new MemoryCache<object>(logger);
//            _logger = logger;
//            _itemThresholdCount = itemThresholdCount;
//        }

//        public bool AddItem(string key, object item)
//        {
//            var itemAdded = _cache.TryAddItem(key, item);
//            var result = (itemAdded) ? "added" : "not added";
//            _logger.Info($"Item {key} {result}");
//            return itemAdded;
//        }

//        public object GetItem(string key)
//        {
//            var itemRetrieved = _cache.TryGetItem(key, out var item);
//            var result = (itemRetrieved) ? "retrieved" : "not retrieved";
//            _logger.Warn($"Item {key} {result}");
//            return item;
//        }

//        public void Dispose()
//        {
//            _cache.Dispose();
//        }
//    }
//}
