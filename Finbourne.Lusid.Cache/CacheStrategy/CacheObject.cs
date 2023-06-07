namespace Finbourne.Lusid.Cache.CacheStrategy
{
    public class CacheObject<T> : IDisposable
    {
        private int _usageCount;
        private T _cachedItem;

        #region Model Attributes
        public string Key { get; set; }
        public int UsageCount => _usageCount;
        public T GetItem
        {
            get 
            {  
                Interlocked.Increment(ref _usageCount);
                return _cachedItem; 
            }
        }
            
        public T AddItem
        {
            set
            {
                _cachedItem = value;
            }
        }
        #endregion Model Attributes

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (_cachedItem is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
