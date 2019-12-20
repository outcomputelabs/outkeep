using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Outkeep.Core
{
    /// <summary>
    /// Quality-of-life extensions for <see cref="ConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Removes a key and value from the dictionary if both match.
        /// </summary>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> entry)
        {
            if (dictionary == null) ThrowDictionaryNull();

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(entry);

            static void ThrowDictionaryNull() => throw new ArgumentNullException(nameof(dictionary));
        }
    }
}