using Microsoft.Extensions.Options;
using Outkeep.Core.Properties;
using Outkeep.Core.Storage;
using System;

namespace Outkeep.Core
{
    public class JsonFileCacheStorageOptionsValidator : IValidateOptions<JsonFileCacheStorageOptions>
    {
        public ValidateOptionsResult Validate(string name, JsonFileCacheStorageOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // storage directory is required
            if (string.IsNullOrWhiteSpace(options.StorageDirectory))
            {
                return ValidateOptionsResult.Fail(Resources.Exception_CacheStorageOptions_StorageDirectory_MustBeConfigured);
            }

            // storage directory must be well formed
            if (!Uri.IsWellFormedUriString(options.StorageDirectory, UriKind.Absolute))
            {
                return ValidateOptionsResult.Fail(Resources.Exception_CacheStorageOptions_StorageDirectory_X_IsNotWellAbsoluteFormed.Format(options.StorageDirectory));
            }

            return ValidateOptionsResult.Success;
        }
    }
}