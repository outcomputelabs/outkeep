using Outkeep.Core.Properties;
using System.ComponentModel.DataAnnotations;

namespace Outkeep.Core.Storage
{
    public class JsonFileCacheStorageOptions
    {
        /// <summary>
        /// The directory to use for storing files.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.Exception_CacheStorageOptions_StorageDirectory_MustBeConfigured))]
        [MinLength(1, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.Exception_CacheStorageOptions_StorageDirectory_MustBeConfigured))]
        public string StorageDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The buffer size used to write files to storage.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int BufferSize { get; set; } = 4096;
    }
}