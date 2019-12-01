using Microsoft.Extensions.Options;

namespace Outkeep.Core
{
    public class FileCacheStorageOptionsValidator : IValidateOptions<FileCacheStorageOptions>
    {
        public ValidateOptionsResult Validate(string name, FileCacheStorageOptions options)
        {
            return ValidateOptionsResult.Success;
        }
    }
}