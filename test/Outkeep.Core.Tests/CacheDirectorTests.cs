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
            var director = new CacheDirector(Options.Create(options), NullLogger<CacheDirector>.Instance, NullClock.Default);

            // assert
            Assert.Equal(0, director.Count);
            Assert.Equal(0, director.Size);
            Assert.Equal(options.MaxCapacity, director.MaxCapacity);
            Assert.Equal(options.TargetCapacity, director.TargetCapacity);
        }

        [Fact]
        public void EntryLifecycle()
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
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector(Options.Create(options), NullLogger<CacheDirector>.Instance, clock);
            var key = "SomeKey";
            var size = 1000;

            // act
            var entry = director.CreateEntry(key, size);

            // assert
            Assert.NotNull(entry);
            Assert.Equal(key, entry.Key);
            Assert.Equal(size, entry.Size);
            Assert.Null(entry.AbsoluteExpiration);
            Assert.Equal(EvictionCause.None, entry.EvictionCause);
            Assert.False(entry.IsExpired);
            Assert.Null(entry.SlidingExpiration);
            Assert.Equal(DateTimeOffset.MinValue, entry.UtcLastAccessed);
            Assert.Equal(0, director.Count);
            Assert.Equal(0, director.Size);

            // act
            entry.Commit();

            // assert
            Assert.Equal(1, director.Count);
            Assert.Equal(size, director.Size);

            // act
            entry.Expire();
            Assert.Equal(0, director.Count);
            Assert.Equal(0, director.Size);
        }

        [Fact]
        public void DoesNotCreateEntriesUnderUnitSize()
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
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector(Options.Create(options), NullLogger<CacheDirector>.Instance, clock);
            var key = "SomeKey";
            var size = 0;

            // act
            Assert.Throws<ArgumentOutOfRangeException>(nameof(size), () => director.CreateEntry(key, size));
        }

        [Fact]
        public void DoesNotCreateEntriesAboveTargetCapacity()
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
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector(Options.Create(options), NullLogger<CacheDirector>.Instance, clock);
            var key = "SomeKey";
            var size = 9000;

            // act
            Assert.Throws<ArgumentOutOfRangeException>(nameof(size), () => director.CreateEntry(key, size));
        }
    }
}