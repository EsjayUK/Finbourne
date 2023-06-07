using Finbourne.Lusid.Cache.Models.Exceptions;

namespace Finbourne.Lusid.Cache.CacheStrategy
{
    public interface ICache<T> : IDisposable
    {
        public bool TryAddItem(string key, T item);
        public bool TryGetItem(string key, out T item);
        public bool KeyExists(string key);        
        public int UsageCount(string key);
        public int ItemCount { get; }
        public event EventHandler<CacheObjectRemovedEventArgs> CacheObjectRemovedEvent;
    }
}
