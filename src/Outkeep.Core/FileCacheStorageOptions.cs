using System.ComponentModel.DataAnnotations;

namespace Outkeep.Core
{
    public class FileCacheStorageOptions
    {
        [Required]
        public string? StorageDirectory { get; set; }

        [Range(1, int.MaxValue)]
        public int BufferSize { get; set; } = 4096;
    }
}