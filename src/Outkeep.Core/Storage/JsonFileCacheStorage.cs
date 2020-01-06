using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Outkeep.Core.Properties;
using Outkeep.Core.Storage;
using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    /// <summary>
    /// Implements a simple <see cref="ICacheStorage"/> that stores data in individual json files.
    /// </summary>
    internal sealed class JsonFileCacheStorage : ICacheStorage
    {
        private static readonly ImmutableHashSet<char> InvalidChars = Enumerable.Concat(Path.GetInvalidFileNameChars(), Path.GetInvalidPathChars()).ToImmutableHashSet();

        private readonly ILogger<JsonFileCacheStorage> _logger;
        private readonly JsonFileCacheStorageOptions _options;

        private const string KeyPropertyName = "key";
        private const string ValuePropertyName = "value";
        private const string AbsoluteExpirationPropertyName = "absoluteExpiration";
        private const string SlidingExpirationPropertyName = "slidingExpiration";
        private const string Extension = ".json";
        private const char InvalidFileNameReplacementChar = '_';
        private const char HashSeparator = '.';

        public JsonFileCacheStorage(ILogger<JsonFileCacheStorage> logger, IOptions<JsonFileCacheStorageOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
        private string KeyToFileName(string key)
        {
            Span<char> path = stackalloc char[1 << 10];

            // format the storage directory into the file path
            if (!_options.StorageDirectory.AsSpan().TryCopyTo(path))
            {
                throw new PathTooLongException();
            }
            var count = _options.StorageDirectory.Length;

            // add in the path separator if not there yet
            if (path[count - 1] != Path.DirectorySeparatorChar && path[count - 1] != Path.AltDirectorySeparatorChar)
            {
                path[count++] = Path.DirectorySeparatorChar;
            }

            // format the key into the title by replacing invalid characters
            for (var i = 0; i < key.Length; ++i)
            {
                path[count++] = InvalidChars.Contains(key[i]) ?
                    InvalidFileNameReplacementChar :
                    key[i];
            }

            // add in the hash separator
            path[count++] = HashSeparator;

            // add in the hash itself
            if (!JenkinsHash.ComputeHash(key).TryFormat(path.Slice(count), out var charsWritten))
            {
                throw new PathTooLongException();
            }
            count += charsWritten;

            // add the file extension
            if (!Extension.AsSpan().TryCopyTo(path.Slice(count)))
            {
                throw new PathTooLongException();
            }
            count += Extension.Length;

            // all done
            return path.Slice(0, count).ToString();
        }

        public Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return InnerClearAsync(key);
        }

        private async Task InnerClearAsync(string key)
        {
            // attempt to delete the file in an async fashion as not to block the caller thread
            var path = KeyToFileName(key);
            try
            {
                // this will attempt to open and close the file while letting the operating system delete it upon close
                // we do this instead of file.delete to attempt to use the underlying platform overlapped i/o capability
                using (var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None,
                    _options.BufferSize,
                    FileOptions.DeleteOnClose | FileOptions.Asynchronous))
                {
                    await stream.DisposeAsync();
                }
            }
            catch (FileNotFoundException)
            {
                // this is okay
                _logger.FileCacheStorageFileNotFound(path, key);
                return;
            }
            catch (Exception ex)
            {
                // this is not okay
                var error = new JsonFileCacheStorageException(Resources.Exception_FailedToClearCacheFile_X_ForKey_X.Format(path, key), path, key, ex);
                Log.FileCacheStorageFailed(_logger, path, key, error);
                throw error;
            }

            Log.FileCacheStorageDeletedFile(_logger, path, key);
        }

        public Task<CacheItem?> ReadAsync(string key, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return InnerReadAsync(key, cancellationToken);
        }

        private async Task<CacheItem?> InnerReadAsync(string key, CancellationToken cancellationToken)
        {
            var path = KeyToFileName(key);
            byte[]? value = null;
            DateTimeOffset? absoluteExpiration = null;
            TimeSpan? slidingExpiration = null;

            try
            {
                using (var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    _options.BufferSize,
                    FileOptions.Asynchronous))
                {
                    using (var document = await JsonDocument.ParseAsync(stream, default, cancellationToken).ConfigureAwait(false))
                    {
                        if (document.RootElement.TryGetProperty(KeyPropertyName, out var keyValue))
                        {
                            string readKey = keyValue.GetString();

                            if (readKey == null)
                            {
                                throw new JsonFileCacheStorageException(Resources.Exception_CacheFile_X_ContainsNullKey_YetWeExpected_X.Format(path, key), path, key);
                            }
                            else if (readKey != key)
                            {
                                throw new JsonFileCacheStorageException(Resources.Exception_CacheFile_X_ContainsKey_X_YetWeExpected_X.Format(path, readKey, key), path, key, readKey);
                            }
                        }
                        else
                        {
                            throw new JsonFileCacheStorageException(Resources.Exception_CacheFile_X_DoesNotContainExpectedKey_X.Format(path, key), path, key);
                        }

                        if (document.RootElement.TryGetProperty(ValuePropertyName, out var valueValue))
                        {
                            value = valueValue.GetBytesFromBase64();
                        }
                        else
                        {
                            throw new JsonFileCacheStorageException(Resources.Exception_CacheFile_X_ForKey_X_DoesNotContainValue.Format(path, key));
                        }

                        if (document.RootElement.TryGetProperty(AbsoluteExpirationPropertyName, out var absoluteExpirationValue))
                        {
                            absoluteExpiration = absoluteExpirationValue.GetDateTimeOffset();
                        }

                        if (document.RootElement.TryGetProperty(SlidingExpirationPropertyName, out var slidingExpirationValue))
                        {
                            // this alocates a string for no good reason - refactor this once the json reader supports timespans
                            slidingExpiration = TimeSpan.Parse(slidingExpirationValue.GetString(), CultureInfo.InvariantCulture);
                        }
                    }

                    await stream.DisposeAsync();
                }
            }
            catch (FileNotFoundException)
            {
                // this is okay
                _logger.FileCacheStorageFileNotFound(path, key);
                return null;
            }
            catch (JsonFileCacheStorageException ex)
            {
                // bubble these up
                Log.FileCacheStorageFailed(_logger, path, key, ex);
                throw;
            }
            catch (Exception ex)
            {
                // wrap everything else
                var error = new JsonFileCacheStorageException(Resources.Exception_FailedToReadCacheFile, path, key, ex);
                Log.FileCacheStorageFailed(_logger, path, key, error);
                throw error;
            }

            _logger.FileCacheStorageReadFile(path, key, value.Length);
            return new CacheItem(value, absoluteExpiration, slidingExpiration);
        }

        public Task WriteAsync(string key, CacheItem item, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return InnerWriteAsync(key, item, cancellationToken);
        }

        private async Task InnerWriteAsync(string key, CacheItem item, CancellationToken cancellationToken)
        {
            var path = KeyToFileName(key);
            long size;
            try
            {
                // open the storage file for async writing
                using (var stream = new FileStream(
                    path,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    default,
                    FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.WriteThrough))
                {
                    // clear any existing data
                    stream.SetLength(0);

                    // todo: the writer supports resetting to a new stream so create an ownership pool to reduce allocations
                    using (var writer = new Utf8JsonWriter(stream))
                    {
                        writer.WriteStartObject();

                        writer.WriteString(KeyPropertyName, key);
                        writer.WriteBase64String(ValuePropertyName, item.Value);

                        if (item.AbsoluteExpiration.HasValue)
                        {
                            writer.WriteString(AbsoluteExpirationPropertyName, item.AbsoluteExpiration.Value);
                        }

                        if (item.SlidingExpiration.HasValue)
                        {
                            // temporary approach to reduce allocations due to lack of timespan support from the writer
                            using var buffer = MemoryPool<char>.Shared.Rent(100);
                            if (item.SlidingExpiration.Value.TryFormat(buffer.Memory.Span, out var charsWritten))
                            {
                                writer.WriteString(SlidingExpirationPropertyName, buffer.Memory.Span.Slice(0, charsWritten));
                            }
                        }

                        writer.WriteEndObject();

                        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                        size = writer.BytesCommitted;

                        await writer.DisposeAsync();
                    }

                    await stream.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                var error = new JsonFileCacheStorageException(Resources.Exception_FailedToWriteCacheFile_X_For_Key_X.Format(path, key), path, key, ex);
                Log.FileCacheStorageFailed(_logger, path, key, error);
                throw error;
            }

            _logger.FileCacheStorageWroteFile(path, key, size);
        }

        private static class Log
        {
            #region Failed

            private static readonly Action<ILogger, string, string, Exception> _fileCacheStorageFailed =
                LoggerMessage.Define<string, string>(
                    LogLevel.Error,
                    new EventId(0, nameof(FileCacheStorageFailed)),
                    Resources.Log_FailedOperationOnCacheFile_X_ForKey_X);

            public static void FileCacheStorageFailed(ILogger logger, string path, string key, Exception exception) =>
                _fileCacheStorageFailed(logger, path, key, exception);

            #endregion Failed

            #region DeletedFile

            private static readonly Action<ILogger, string, string, Exception?> _fileCacheStorageDeletedFileForKey =
                LoggerMessage.Define<string, string>(
                    LogLevel.Debug,
                    new EventId(0, nameof(FileCacheStorageDeletedFile)),
                    Resources.Log_DeletedCacheFile_X_ForKey_X);

            public static void FileCacheStorageDeletedFile(ILogger logger, string path, string key) =>
                _fileCacheStorageDeletedFileForKey(logger, path, key, null);

            #endregion DeletedFile
        }
    }
}