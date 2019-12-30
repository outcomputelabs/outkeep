namespace Outkeep.Core.Caching
{
    internal interface ICacheContext
    {
        /// <summary>
        /// Invoked by each entry when being committed.
        /// </summary>
        /// <param name="entry">The entry being committed.</param>
        public void OnEntryCommitted(CacheEntry entry);

        /// <summary>
        /// Invoked by each entry after self-expiry.
        /// </summary>
        /// <param name="entry">The entry that has expired.</param>
        public void OnEntryExpired(CacheEntry entry);
    }
}