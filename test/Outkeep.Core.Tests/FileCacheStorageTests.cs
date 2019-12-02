using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Outkeep.Core.Tests
{
    public class FileCacheStorageTests
    {
        private static readonly ImmutableHashSet<char> InvalidFileNameChars = Path.GetInvalidFileNameChars().ToImmutableHashSet();
        private const char InvalidFileNameReplacementChar = '_';
        private const string Extension = ".json";

        private static string KeyToFileTitle(string key)
        {
            return string.Create(key.Length + Extension.Length, key, (output, input) =>
            {
                for (var i = 0; i < input.Length; ++i)
                {
                    output[i] = InvalidFileNameChars.Contains(input[i]) ?
                        InvalidFileNameReplacementChar :
                        input[i];
                }

                Extension.AsSpan().CopyTo(output.Slice(input.Length));
            });
        }

        [Fact]
        public async Task ClearDeletesFile()
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
    }
}