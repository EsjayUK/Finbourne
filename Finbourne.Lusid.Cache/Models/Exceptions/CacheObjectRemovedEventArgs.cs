namespace Finbourne.Lusid.Cache.Models.Exceptions
{
    public class CacheObjectRemovedEventArgs : EventArgs
    {
        public string Key { get; set; }
        public int ThresholdUsageCount { get; set; }
    }
}
