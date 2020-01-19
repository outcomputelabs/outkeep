﻿using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Outkeep.Core.Caching;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class CacheDirectorTests
    {
        [Fact]
        public void Initializes()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 10000,
            };

            // act
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, NullClock.Default);

            // assert
            Assert.Equal(0, director.Count);
            Assert.Equal(0, director.Size);
            Assert.Equal(options.Capacity, director.Capacity);
        }

        [Fact]
        public void EntryLifecycle()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 10000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);
            var key = "SomeKey";
            var size = 1000;

            // act
            var entry = director.CreateEntry(key, size);

            // assert
            Assert.NotNull(entry);
            Assert.Equal(key, entry.Key);
            Assert.Equal(size, entry.Size);
            Assert.Equal(EvictionCause.None, entry.EvictionCause);
            Assert.False(entry.IsExpired);
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
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 10000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);
            var key = "SomeKey";
            var size = 0;

            // act
            Assert.Throws<ArgumentOutOfRangeException>(nameof(size), () => director.CreateEntry(key, size));
        }

        [Fact]
        public void CreateEntryThrowsOnNullKey()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 10000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);
            string? key = null;

            // act
            void action() { director.CreateEntry(key!, 1000); }

            // assert
            Assert.Throws<ArgumentNullException>(nameof(key), action);
        }

        [Fact]
        public void ReplacesExistingEntry()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 10000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);
            var key = "SomeKey";
            var size1 = 1000;
            var size2 = 2000;

            // act
            var entry1 = director.CreateEntry(key, size1).Commit();

            // assert
            Assert.NotNull(entry1);
            Assert.Equal(EvictionCause.None, entry1.EvictionCause);
            Assert.False(entry1.IsExpired);
            Assert.Equal(1, director.Count);
            Assert.Equal(size1, director.Size);

            // act
            var entry2 = director.CreateEntry(key, size2).Commit();

            // assert
            Assert.Equal(EvictionCause.Replaced, entry1.EvictionCause);
            Assert.True(entry1.IsExpired);

            Assert.NotNull(entry2);
            Assert.Equal(EvictionCause.None, entry2.EvictionCause);
            Assert.False(entry2.IsExpired);
            Assert.Equal(1, director.Count);
            Assert.Equal(size2, director.Size);
        }

        [Fact]
        public void ExpiresPreviousEntryOnCapacityFailure()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 10000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);
            var key = "SomeKey";

            // act
            var entry1 = director.CreateEntry(key, 10000).SetPriority(CachePriority.NeverRemove).Commit();

            // assert
            Assert.Equal(1, director.Count);
            Assert.Equal(entry1.Size, director.Size);
            Assert.False(entry1.IsExpired);

            // act
            var entry2 = director.CreateEntry(key, 10000).SetPriority(CachePriority.NeverRemove).Commit();

            // assert
            Assert.Equal(0, director.Count);
            Assert.Equal(0, director.Size);

            Assert.True(entry1.IsExpired);
            Assert.Equal(EvictionCause.Replaced, entry1.EvictionCause);

            Assert.True(entry2.IsExpired);
            Assert.Equal(EvictionCause.Capacity, entry2.EvictionCause);
        }

        [Fact]
        public void ExpiresConcurrentEntryOnConflict()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 10000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);
            var key = "SomeKey";
            var size = 10;

            Parallel.For(0, 100000, _ =>
            {
                director
                    .CreateEntry(key, size)
                    .Commit();
            });

            // assert
            Assert.Equal(1, director.Count);
            Assert.Equal(size, director.Size);
        }

        [Fact]
        public void CompactRemovesLowPriorityEntriesFirst()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 14000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);

            // act
            var entry1 = director.CreateEntry("Key1", 1000).SetPriority(CachePriority.Low).Commit();
            var entry2 = director.CreateEntry("Key2", 2000).SetPriority(CachePriority.Normal).Commit();
            var entry3 = director.CreateEntry("Key3", 4000).SetPriority(CachePriority.High).Commit();

            // assert
            Assert.Equal(3, director.Count);
            Assert.Equal(entry1.Size + entry2.Size + entry3.Size, director.Size);

            // act
            var entry4 = director.CreateEntry("Key4", 8000).SetPriority(CachePriority.High).Commit();

            // assert
            Assert.Equal(3, director.Count);
            Assert.Equal(entry2.Size + entry3.Size + entry4.Size, director.Size);
            Assert.True(entry1.IsExpired);
            Assert.False(entry2.IsExpired);
            Assert.False(entry3.IsExpired);
            Assert.False(entry4.IsExpired);
        }

        [Fact]
        public void CompactRemovesNormalPriorityEntriesNext()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 13000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);

            // act
            var entry1 = director.CreateEntry("Key1", 1000).SetPriority(CachePriority.Low).Commit();
            var entry2 = director.CreateEntry("Key2", 2000).SetPriority(CachePriority.Normal).Commit();
            var entry3 = director.CreateEntry("Key3", 4000).SetPriority(CachePriority.High).Commit();

            // assert
            Assert.Equal(3, director.Count);
            Assert.Equal(entry1.Size + entry2.Size + entry3.Size, director.Size);

            // act
            var entry4 = director.CreateEntry("Key4", 8000).SetPriority(CachePriority.Normal).Commit();

            // assert
            Assert.Equal(2, director.Count);
            Assert.Equal(entry3.Size + entry4.Size, director.Size);
            Assert.True(entry1.IsExpired);
            Assert.True(entry2.IsExpired);
            Assert.False(entry3.IsExpired);
            Assert.False(entry4.IsExpired);
        }

        [Fact]
        public void CompactRemovesHighPriorityEntriesNext()
        {
            // arrange
            var options = new CacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1),
                Capacity = 10000,
            };
            var clock = new NullClock
            {
                UtcNow = DateTimeOffset.UtcNow
            };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, clock);

            // act
            var entry1 = director.CreateEntry("Key1", 1000).SetPriority(CachePriority.Low).Commit();
            var entry2 = director.CreateEntry("Key2", 2000).SetPriority(CachePriority.Normal).Commit();
            var entry3 = director.CreateEntry("Key3", 4000).SetPriority(CachePriority.High).Commit();

            // assert
            Assert.Equal(3, director.Count);
            Assert.Equal(entry1.Size + entry2.Size + entry3.Size, director.Size);

            // act
            var entry4 = director.CreateEntry("Key4", 8000).SetPriority(CachePriority.High).Commit();

            // assert
            Assert.Equal(1, director.Count);
            Assert.Equal(entry4.Size, director.Size);
            Assert.True(entry1.IsExpired);
            Assert.True(entry2.IsExpired);
            Assert.True(entry3.IsExpired);
            Assert.False(entry4.IsExpired);
        }

        [Fact]
        public void CreateEntryThrowsOnSizeTooLarge()
        {
            // arrange
            var options = new CacheOptions { Capacity = 1000 };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, NullClock.Default);
            var key = Guid.NewGuid().ToString();
            var size = options.Capacity + 1;

            // act
            void action() => director.CreateEntry(key, 1001);

            // assert
            Assert.Throws<ArgumentOutOfRangeException>(nameof(size), action);
        }

        [Fact]
        public void TryGetEntryThrowsOnNullKey()
        {
            // arrange
            var options = new CacheOptions();
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, NullClock.Default);
            string? key = null;

            // act
            void action() => director.TryGetEntry(null!, out _);

            // assert
            Assert.Throws<ArgumentNullException>(nameof(key), action);
        }

        [Fact]
        public void TryGetEntryHandlesNonExistingEntry()
        {
            // arrange
            var options = new CacheOptions();
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, NullClock.Default);
            var key = Guid.NewGuid().ToString();

            // act
            var result = director.TryGetEntry(key, out var entry);

            // assert
            Assert.False(result);
            Assert.Null(entry);
        }

        [Fact]
        public void ExpiresEntryOnOverflow()
        {
            // arrange
            var options = new CacheOptions { Capacity = long.MaxValue };
            var director = new CacheDirector<string>(Options.Create(options), NullLogger<CacheDirector<string>>.Instance, NullClock.Default);
            var key = Guid.NewGuid().ToString();

            // act
            var filler = director
                .CreateEntry(key, long.MaxValue - 100)
                .Commit();

            // assert
            Assert.NotNull(filler);
            Assert.False(filler.IsExpired);

            // act
            var entry = director
                .CreateEntry(key, 1000)
                .Commit();

            // assert
            Assert.NotNull(entry);
            Assert.True(entry.IsExpired);
            Assert.True(filler.IsExpired);
        }
    }
}