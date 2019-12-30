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

            if (options.StorageDirectory != null)
            {
                if (!Uri.IsWellFormedUriString(options.StorageDirectory, UriKind.Absolute))
                {
                    return ValidateOptionsResult.Fail(Resources.Exception_CacheStorageOptions_StorageDirectory_X_IsNotWellAbsoluteFormed.Format(options.StorageDirectory));
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}