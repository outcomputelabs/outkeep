﻿using Microsoft.Extensions.Logging.Abstractions;
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

        [Fact]
        public void EvictExpiredEntries()
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

            // act
            var entry1 = director.CreateEntry("SomeKey1", 1).SetAbsoluteExpiration(clock.UtcNow.AddMinutes(1)).Commit();
            var entry2 = director.CreateEntry("SomeKey2", 2).SetAbsoluteExpiration(clock.UtcNow.AddMinutes(3)).Commit();
            var entry3 = director.CreateEntry("SomeKey3", 4).SetSlidingExpiration(TimeSpan.FromMinutes(5)).Commit();
            var entry4 = director.CreateEntry("SomeKey4", 8).SetSlidingExpiration(TimeSpan.FromMinutes(7)).Commit();

            // assert
            Assert.Equal(4, director.Count);
            Assert.Equal(entry1.Size + entry2.Size + entry3.Size + entry4.Size, director.Size);
            Assert.False(entry1.IsExpired);
            Assert.False(entry2.IsExpired);
            Assert.False(entry3.IsExpired);
            Assert.False(entry4.IsExpired);

            // act
            director.EvictExpiredEntries();

            // assert
            Assert.Equal(4, director.Count);
            Assert.Equal(entry1.Size + entry2.Size + entry3.Size + entry4.Size, director.Size);
            Assert.False(entry1.IsExpired);
            Assert.False(entry2.IsExpired);
            Assert.False(entry3.IsExpired);
            Assert.False(entry4.IsExpired);

            // act
            clock.UtcNow = clock.UtcNow.AddMinutes(2);
            director.EvictExpiredEntries();

            // assert
            Assert.Equal(3, director.Count);
            Assert.Equal(entry2.Size + entry3.Size + entry4.Size, director.Size);
            Assert.True(entry1.IsExpired);
            Assert.False(entry2.IsExpired);
            Assert.False(entry3.IsExpired);
            Assert.False(entry4.IsExpired);

            // act
            clock.UtcNow = clock.UtcNow.AddMinutes(2);
            director.EvictExpiredEntries();

            // assert
            Assert.Equal(2, director.Count);
            Assert.Equal(entry3.Size + entry4.Size, director.Size);
            Assert.True(entry1.IsExpired);
            Assert.True(entry2.IsExpired);
            Assert.False(entry3.IsExpired);
            Assert.False(entry4.IsExpired);

            // act
            clock.UtcNow = clock.UtcNow.AddMinutes(2);
            director.EvictExpiredEntries();

            // assert
            Assert.Equal(1, director.Count);
            Assert.Equal(entry4.Size, director.Size);
            Assert.True(entry1.IsExpired);
            Assert.True(entry2.IsExpired);
            Assert.True(entry3.IsExpired);
            Assert.False(entry4.IsExpired);

            // act
            clock.UtcNow = clock.UtcNow.AddMinutes(2);
            entry4.UtcLastAccessed = clock.UtcNow.AddMinutes(1);
            director.EvictExpiredEntries();

            // assert
            Assert.Equal(1, director.Count);
            Assert.Equal(entry4.Size, director.Size);
            Assert.True(entry1.IsExpired);
            Assert.True(entry2.IsExpired);
            Assert.True(entry3.IsExpired);
            Assert.False(entry4.IsExpired);

            // act
            clock.UtcNow = entry4.UtcLastAccessed.Add(entry4.SlidingExpiration.GetValueOrDefault()).AddMinutes(1);
            director.EvictExpiredEntries();

            // assert
            Assert.Equal(0, director.Count);
            Assert.Equal(0, director.Size);
            Assert.True(entry1.IsExpired);
            Assert.True(entry2.IsExpired);
            Assert.True(entry3.IsExpired);
            Assert.True(entry4.IsExpired);
        }
    }
}