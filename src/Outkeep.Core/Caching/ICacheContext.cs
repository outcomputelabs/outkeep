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
        /// Invoked by each entry after self-revocation.
        /// </summary>
        /// <param name="entry">The entry that has self-revoked.</param>
        public void OnEntryRevoked(CacheEntry<TKey> entry);
    }
}