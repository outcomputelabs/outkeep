using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Orleans;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
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
                var logger = new NullLogger<FileCacheStorage>();
                var options = new FileCacheStorageOptions
                {
                    StorageDirectory = directory
                };
                var storage = new FileCacheStorage(logger, Options.Create(options));

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

            var logger = new NullLogger<FileCacheStorage>();
            var options = new FileCacheStorageOptions
            {
                StorageDirectory = directory
            };
            var storage = new FileCacheStorage(logger, Options.Create(options));

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

                var logger = new NullLogger<FileCacheStorage>();
                var options = new FileCacheStorageOptions
                {
                    StorageDirectory = directory
                };
                var storage = new FileCacheStorage(logger, Options.Create(options));

                var error = await Assert.ThrowsAsync<FileCacheStorageException>(() => storage.ClearAsync(key))
                    .ConfigureAwait(false);

                Assert.Equal(key, error.Key);
                Assert.Equal(path, error.Path);
                Assert.IsType<IOException>(error.InnerException);
            }
        }
    }
}