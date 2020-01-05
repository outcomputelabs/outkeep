using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Outkeep.Core.Caching;
using System;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class CacheDirectorTests
    {
        [Fact]
        public void Initializes()
        {
            // arrange
            var options = new CacheDirectorOptions
            {
                AutomaticOvercapacityCompaction = true,
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                OvercapacityCompactionFrequency = TimeSpan.FromMinutes(1),
                MaxCapacity = 10000,
                TargetCapacity = 8000
            };

            // act
            var director = new CacheDirector(Options.Create(options), NullLogger<CacheDirector>.Instance, NullClock.Instance);

            // assert
            Assert.Equal(0, director.Count);
            Assert.Equal(0, director.Size);
            Assert.Equal(options.MaxCapacity, director.MaxCapacity);
            Assert.Equal(options.TargetCapacity, director.TargetCapacity);
        }
    }
}