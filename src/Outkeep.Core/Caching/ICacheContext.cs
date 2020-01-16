namespace Outkeep.Core.Caching
{
    internal interface ICacheContext<TKey> where TKey : notnull
    {
        /// <summary>
        /// Invoked by each entry when being committed.
        /// </summary>
        /// <param name="entry">The entry being committed.</param>
        public void OnEntryCommitted(CacheEntry<TKey> entry);

        /// <summary>
        /// Invoked by each entry after self-expiry.
        /// </summary>
        /// <param name="entry">The entry that has expired.</param>
        public void OnEntryExpired(CacheEntry<TKey> entry);
    }
}