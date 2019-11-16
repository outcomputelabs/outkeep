using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Outkeep.Api.Http.Models.V1
{
    /// <summary>
    /// Item to cache.
    /// </summary>
    [DataContract]
    public class CacheItem
    {
        [DataMember(Name = "key")]
        [Required]
        [MaxLength(128)]
        public string Key { get; set; }

        [DataMember(Name = "value")]
        [Required]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "DTO")]
        public byte[] Value { get; set; }

        [DataMember(Name = "absoluteExpiration")]
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        [DataMember(Name = "slidingExpiration")]
        public TimeSpan? SlidingExpiration { get; set; }
    }
}