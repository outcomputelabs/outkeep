using System.ComponentModel.DataAnnotations;

namespace Outkeep.Core.Storage
{
    public class JsonFileCacheStorageOptions
    {
        /// <summary>
        /// The directory to use for storing files.
        /// </summary>
        [Required]
        [MinLength(1)]
        public string StorageDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The buffer size used to write files to storage.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int BufferSize { get; set; } = 4096;
    }
}