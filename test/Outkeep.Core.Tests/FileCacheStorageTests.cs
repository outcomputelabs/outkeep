using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Orleans;
using Outkeep.Core.Properties;
using Outkeep.Core.Storage;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class FileCacheStorageTests
    {
        private static readonly ImmutableHashSet<char> InvalidFileNameChars = Path.GetInvalidFileNameChars().ToImmutableHashSet();
        private const char InvalidFileNameReplacementChar = '_';

        private static string ReplaceInvalidFileNameChars(string str)
        {
            return string.Create(str.Length, str, (output, input) =>
            {
                for (var i = 0; i < input.Length; ++i)
                {
                    output[i] = InvalidFileNameChars.Contains(input[i]) ?
                        InvalidFileNameReplacementChar :
                        input[i];
                }
            });
        }

        private static string KeyToFileTitle(string key)
        {
            var hash = JenkinsHash.ComputeHash(key);
            return $"{ReplaceInvalidFileNameChars(key)}.{hash}.json";
        }

        [Fact]
        public async Task ClearDeletesExistingFile()
        {
            var key = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var title = KeyToFileTitle(key);
            var directory = Path.GetTempPath();
            var path = Path.Combine(directory, title);

            await File.WriteAllBytesAsync(path, Array.Empty<byte>()).ConfigureAwait(false);
            Assert.True(File.Exists(path));

            try
            {
                var logger = new NullLogger<JsonFileCacheStorage>();
                var options = new JsonFileCacheStorageOptions
                {
                    StorageDirectory = directory
                };
                var storage = new JsonFileCacheStorage(logger, Options.Create(options));

                await storage.ClearAsync(key).ConfigureAwait(false);

                Assert.False(File.Exists(path));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task ClearIgnoresNonExistingFile()
        {
            var key = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var title = KeyToFileTitle(key);
            var directory = Path.GetTempPath();
            var path = Path.Combine(directory, title);

            Assert.False(File.Exists(path));

            var logger = new NullLogger<JsonFileCacheStorage>();
            var options = new JsonFileCacheStorageOptions
            {
                StorageDirectory = directory
            };
            var storage = new JsonFileCacheStorage(logger, Options.Create(options));

            await storage.ClearAsync(key).ConfigureAwait(false);

            Assert.False(File.Exists(path));
        }

        [Fact]
        public async Task ClearFailsOnError()
        {
            var key = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var title = KeyToFileTitle(key);
            var directory = Path.GetTempPath();
            var path = Path.Combine(directory, title);

            // keep the file open
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.DeleteOnClose))
            {
                Assert.True(File.Exists(path));

                var logger = new NullLogger<JsonFileCacheStorage>();
                var options = new JsonFileCacheStorageOptions
                {
                    StorageDirectory = directory
                };
                var storage = new JsonFileCacheStorage(logger, Options.Create(options));

                var error = await Assert.ThrowsAsync<JsonFileCacheStorageException>(() => storage.ClearAsync(key))
                    .ConfigureAwait(false);

                Assert.Equal(key, error.Key);
                Assert.Equal(path, error.Path);
                Assert.IsType<IOException>(error.InnerException);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ReadReturnsCorrectContent(bool trimDirectoryTrailingSeparator)
        {
            var key = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var title = KeyToFileTitle(key);
            var directory = Path.GetTempPath();

            if (trimDirectoryTrailingSeparator)
            {
                while (Path.EndsInDirectorySeparator(directory))
                {
                    directory = Path.TrimEndingDirectorySeparator(directory);
                }
            }

            var path = Path.Combine(directory, title);
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow;
            var slidingExpiration = TimeSpan.FromMinutes(1);

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteString(nameof(key), key);
                writer.WriteString(nameof(absoluteExpiration), absoluteExpiration);
                writer.WriteString(nameof(slidingExpiration), slidingExpiration.ToString());
                writer.WriteBase64String(nameof(value), value);
                writer.WriteEndObject();

                await writer.DisposeAsync();
                await stream.DisposeAsync();
            }

            try
            {
                var logger = new NullLogger<JsonFileCacheStorage>();
                var options = new JsonFileCacheStorageOptions
                {
                    StorageDirectory = directory
                };
                var storage = new JsonFileCacheStorage(logger, Options.Create(options));

                var result = await storage.ReadAsync(key).ConfigureAwait(false);

                Assert.True(result.HasValue);
                Assert.Equal(absoluteExpiration, result.Value.AbsoluteExpiration);
                Assert.Equal(slidingExpiration, result.Value.SlidingExpiration);
                Assert.True(value.SequenceEqual(result.Value.Value));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task ReadThrowsOnIncorrectKey()
        {
            var key = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var otherKey = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var title = KeyToFileTitle(key);
            var directory = Path.GetTempPath();
            var path = Path.Combine(directory, title);
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow;
            var slidingExpiration = TimeSpan.FromMinutes(1);

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteString(nameof(key), otherKey);
                writer.WriteString(nameof(absoluteExpiration), absoluteExpiration);
                writer.WriteString(nameof(slidingExpiration), slidingExpiration.ToString());
                writer.WriteBase64String(nameof(value), value);
                writer.WriteEndObject();

                await writer.DisposeAsync();
                await stream.DisposeAsync();
            }

            try
            {
                var logger = new NullLogger<JsonFileCacheStorage>();
                var options = new JsonFileCacheStorageOptions
                {
                    StorageDirectory = directory
                };
                var storage = new JsonFileCacheStorage(logger, Options.Create(options));

                var error = await Assert.ThrowsAsync<JsonFileCacheStorageException>(() => storage.ReadAsync(key)).ConfigureAwait(false);

                Assert.Equal(Resources.Exception_CacheFile_X_ContainsKey_X_YetWeExpected_X.Format(path, otherKey, key), error.Message);
                Assert.Equal(path, error.Path);
                Assert.Equal(key, error.Key);
                Assert.Equal(otherKey, error.OtherKey);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task ReadThrowsOnMissingKey()
        {
            var key = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var title = KeyToFileTitle(key);
            var directory = Path.GetTempPath();
            var path = Path.Combine(directory, title);
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow;
            var slidingExpiration = TimeSpan.FromMinutes(1);

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteString(nameof(absoluteExpiration), absoluteExpiration);
                writer.WriteString(nameof(slidingExpiration), slidingExpiration.ToString());
                writer.WriteBase64String(nameof(value), value);
                writer.WriteEndObject();

                await writer.DisposeAsync();
                await stream.DisposeAsync();
            }

            try
            {
                var logger = new NullLogger<JsonFileCacheStorage>();
                var options = new JsonFileCacheStorageOptions
                {
                    StorageDirectory = directory
                };
                var storage = new JsonFileCacheStorage(logger, Options.Create(options));

                var error = await Assert.ThrowsAsync<JsonFileCacheStorageException>(() => storage.ReadAsync(key)).ConfigureAwait(false);

                Assert.Equal(Resources.Exception_CacheFile_X_DoesNotContainExpectedKey_X.Format(path, key), error.Message);
                Assert.Equal(path, error.Path);
                Assert.Equal(key, error.Key);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task ReadThrowsOnNullKey()
        {
            var key = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var title = KeyToFileTitle(key);
            var directory = Path.GetTempPath();
            var path = Path.Combine(directory, title);
            var value = Guid.NewGuid().ToByteArray();
            var absoluteExpiration = DateTimeOffset.UtcNow;
            var slidingExpiration = TimeSpan.FromMinutes(1);

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteNull(nameof(key));
                writer.WriteString(nameof(absoluteExpiration), absoluteExpiration);
                writer.WriteString(nameof(slidingExpiration), slidingExpiration.ToString());
                writer.WriteBase64String(nameof(value), value);
                writer.WriteEndObject();

                await writer.DisposeAsync();
                await stream.DisposeAsync();
            }

            try
            {
                var logger = new NullLogger<JsonFileCacheStorage>();
                var options = new JsonFileCacheStorageOptions
                {
                    StorageDirectory = directory
                };
                var storage = new JsonFileCacheStorage(logger, Options.Create(options));

                var error = await Assert.ThrowsAsync<JsonFileCacheStorageException>(() => storage.ReadAsync(key)).ConfigureAwait(false);

                Assert.Equal(Resources.Exception_CacheFile_X_ContainsNullKey_YetWeExpected_X.Format(path, key), error.Message);
                Assert.Equal(path, error.Path);
                Assert.Equal(key, error.Key);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}