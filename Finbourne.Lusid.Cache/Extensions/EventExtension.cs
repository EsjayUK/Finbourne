namespace Finbourne.Lusid.Cache.Extensions
{
    internal static class EventExtension
    {
        public static void Raise<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
        {
            handler?.Invoke(sender, args);
        }
    }
}
