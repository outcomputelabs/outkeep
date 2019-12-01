using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    public class FileCacheStorage : ICacheStorage
    {
        public FileCacheStorage(ILogger<FileCacheStorage> logger, IOptions<FileCacheStorageOptions> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        private readonly ILogger<FileCacheStorage> logger;
        private readonly FileCacheStorageOptions options;

        private string KeyToFileName(string key)
        {
            // todo: refactor this allocation happy code into span based formatting
            return $"{Path.Combine(options.StorageDirectory, BitConverter.ToString(Encoding.UTF8.GetBytes(key)))}.json";
        }

        public async Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            using var stream = new FileStream(
                KeyToFileName(key),
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.None,
                default,
                FileOptions.DeleteOnClose | FileOptions.Asynchronous);

            await stream.DisposeAsync();
        }

        public async Task<(byte[] Value, DateTimeOffset? AbsoluteExpiration, TimeSpan? SlidingExpiration)?> TryReadAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                using var stream = new FileStream(
                    KeyToFileName(key),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    default,
                    FileOptions.Asynchronous);

                using var document = await JsonDocument.ParseAsync(stream, default, cancellationToken).ConfigureAwait(false);

                string readKey = null;
                byte[] value = null;
                DateTimeOffset? absoluteExpiration = null;
                TimeSpan? slidingExpiration = null;

                var root = document.RootElement;

                if (root.TryGetProperty(nameof(key), out var keyValue))
                {
                    readKey = keyValue.GetString();
                }

                if (root.TryGetProperty(nameof(value), out var valueValue))
                {
                    value = valueValue.GetBytesFromBase64();
                }

                if (root.TryGetProperty(nameof(absoluteExpiration), out var absoluteExpirationValue))
                {
                    absoluteExpiration = absoluteExpirationValue.GetDateTimeOffset();
                }

                if (root.TryGetProperty(nameof(slidingExpiration), out var slidingExpirationValue))
                {
                    // todo: refactor this into non-allocating code if/when the json reader supports timespan or at least writing to a span
                    slidingExpiration = TimeSpan.Parse(slidingExpirationValue.GetString(), CultureInfo.InvariantCulture);
                }

                await stream.DisposeAsync();

                if (readKey != key || value == null) return null;

                return (value, absoluteExpiration, slidingExpiration);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public async Task WriteAsync(string key, byte[] value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            // open the storage file for async writing
            using var stream = new FileStream(
                KeyToFileName(key),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                default,
                FileOptions.Asynchronous);

            // todo: the writer supports resetting to a new stream so create a pool to reduce allocations
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();
            writer.WriteString("key", key);
            writer.WriteBase64String("value", value);

            if (absoluteExpiration.HasValue)
            {
                writer.WriteString("absoluteExpiration", absoluteExpiration.Value);
            }

            if (slidingExpiration.HasValue)
            {
                // temporary approach to reduce allocations due to lack of timespan support from the writer
                using var buffer = MemoryPool<char>.Shared.Rent(100);
                if (slidingExpiration.Value.TryFormat(buffer.Memory.Span, out var charsWritten))
                {
                    writer.WriteString("slidingExpiration", buffer.Memory.Span.Slice(0, charsWritten));
                }
            }

            writer.WriteEndObject();

            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            await writer.DisposeAsync();
            await stream.DisposeAsync();
        }
    }
}