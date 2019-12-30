using Microsoft.Extensions.Options;
using Outkeep.Core.Properties;
using System;

namespace Outkeep.Core
{
    public class FileCacheStorageOptionsValidator : IValidateOptions<FileCacheStorageOptions>
    {
        public ValidateOptionsResult Validate(string name, FileCacheStorageOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            // storage directory is required
            if (options.StorageDirectory == null)
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