using Microsoft.Extensions.Options;
using System;

namespace Outkeep.Core.Caching
{
    internal class CacheDirectorOptionsValidator : IValidateOptions<CacheDirectorOptions>
    {
        public ValidateOptionsResult Validate(string name, CacheDirectorOptions options)
        {
            return ValidateOptionsResult.Success;
        }
    }
}