namespace Finbourne.Lusid.Cache.CacheStrategy
{
    internal class CacheObject<T> : IDisposable
    {
        private int _usageCount;
        private T _cachedItem;

        #region Model Attributes
        public string Key { get; set; }
#pragma warning disable CA1822 // Mark members as static
        public int UsageCount => _usageCount;
#pragma warning restore CA1822 // Mark members as static
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
            if (_cachedItem is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
