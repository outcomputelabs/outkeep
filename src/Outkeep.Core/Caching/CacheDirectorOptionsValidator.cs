using Microsoft.Extensions.Options;
using System;

namespace Outkeep.Core.Caching
{
    public class CacheDirectorOptionsValidator : IValidateOptions<CacheDirectorOptions>
    {
        public ValidateOptionsResult Validate(string name, CacheDirectorOptions options)
        {
            throw new NotImplementedException();
        }
    }
}