namespace Finbourne.Lusid.Cache.Extensions
{
    internal static class EventExtension
    {
        /// <summary>
        /// Pass in a generic argument object and raise event
        /// </summary>
        /// <typeparam name="T">The type of event argument object</typeparam>
        /// <param name="handler">The event handler used to raise the event</param>
        /// <param name="sender">The class rasising the event</param>
        /// <param name="args">The generic arguments object</param>
        public static void Raise<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
        {
            handler?.Invoke(sender, args);
        }
    }
}
