using System.ComponentModel.DataAnnotations;

namespace Outkeep.Core
{
    public class FileCacheStorageOptions
    {
        [Required]
        public string StorageDirectory { get; set; }
    }
}