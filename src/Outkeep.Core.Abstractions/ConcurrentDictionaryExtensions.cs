using System.Collections.Generic;

namespace System.Collections.Concurrent
{
    /// <summary>
    /// Quality-of-life extensions for <see cref="ConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Removes a key and value from the dictionary if both match.
        /// </summary>
        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> entry)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(entry);
        }
    }
}