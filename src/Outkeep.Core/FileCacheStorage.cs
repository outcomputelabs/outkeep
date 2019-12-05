using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Outkeep.Core.Properties;
using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core
{
    public class FileCacheStorage : ICacheStorage
    {
        private const string KeyPropertyName = "key";
        private const string ValuePropertyName = "value";
        private const string AbsoluteExpirationPropertyName = "absoluteExpiration";
        private const string SlidingExpirationPropertyName = "slidingExpiration";
        private const string Extension = ".json";
        private const char InvalidFileNameReplacementChar = '_';
        private const char HashSeparator = '.';
        private static readonly ImmutableHashSet<char> InvalidFileNameChars = Path.GetInvalidFileNameChars().ToImmutableHashSet();

        public FileCacheStorage(ILogger<FileCacheStorage> logger, IOptions<FileCacheStorageOptions> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        private readonly ILogger<FileCacheStorage> logger;
        private readonly FileCacheStorageOptions options;

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "N/A")]
        private string KeyToFileName(string key)
        {
            Span<char> path = stackalloc char[1 << 10];

            // format the storage directory into the file path
            options.StorageDirectory.AsSpan().CopyTo(path);
            var count = options.StorageDirectory.Length;

            // add in the path separator if not there yet
            if (path[count - 1] != Path.DirectorySeparatorChar && path[count - 1] != Path.AltDirectorySeparatorChar)
            {
                path[count] = Path.DirectorySeparatorChar;
            }

            // format the key into the title by replacing invalid characters
            for (var i = 0; i < key.Length; ++i)
            {
                path[count + i] = InvalidFileNameChars.Contains(key[i]) ?
                    InvalidFileNameReplacementChar :
                    key[i];
            }
            count += key.Length;

            // add in the hash separator
            path[count] = HashSeparator;
            ++count;

            // add in the hash itself
            if (!JenkinsHash.ComputeHash(key).TryFormat(path.Slice(count), out var charsWritten))
            {
                ThrowInvalidOperationException();
            }
            count += charsWritten;

            // add the file extension
            if (!Extension.AsSpan().TryCopyTo(path.Slice(count)))
            {
                ThrowInvalidOperationException();
            }
            count += Extension.Length;

            // all done
            return path.Slice(0, count).ToString();

            static void ThrowInvalidOperationException() => throw new InvalidOperationException();
        }

        public Task ClearAsync(string key, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return InnerClearAsync(key);
        }

        private async Task InnerClearAsync(string key)
        {
            // attempt to delete the file in an async fashion as not to block the caller thread
            // todo: benchmark this to make sure it works across different platforms
            var path = KeyToFileName(key);
            try
            {
                // this will attempt to open and close the file while letting the operating system delete it upon close
                // we do this instead of file.delete to attempt to use the underlying platform overlapped i/o capability
                // todo: test if this happens at all across all platforms and in slower network storage
                using (var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None,
                    options.BufferSize,
                    FileOptions.DeleteOnClose | FileOptions.Asynchronous))
                {
                    await stream.DisposeAsync();
                }
            }
            catch (FileNotFoundException)
            {
                // this is okay
                Log.FileNotFound(logger, path, key);
                return;
            }
            catch (Exception ex)
            {
                // this is not okay
                var error = new FileCacheStorageException(Resources.Exception_FailedToClearCacheFile_X_ForKey_X.Format(path, key), path, key, ex);
                Log.Failed(logger, path, key, error);
                throw error;
            }

            Log.DeletedFile(logger, path, key);
        }

        public Task<CacheItem?> ReadAsync(string key, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            return InnerReadAsync(key, cancellationToken);
        }

        private async Task<CacheItem?> InnerReadAsync(string key, CancellationToken cancellationToken)
        {
            var path = KeyToFileName(key);
            byte[] value = null;
            DateTimeOffset? absoluteExpiration = null;
            TimeSpan? slidingExpiration = null;

            try
            {
                using (var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    options.BufferSize,
                    FileOptions.Asynchronous))
                {
                    using (var document = await JsonDocument.ParseAsync(stream, default, cancellationToken).ConfigureAwait(false))
                    {
                        if (document.RootElement.TryGetProperty(KeyPropertyName, out var keyValue))
                        {
                            string readKey = keyValue.GetString();

                            if (readKey != key)
                            {
                                throw new FileCacheStorageException(Resources.Exception_CacheFile_X_ContainsKey_X_YetWeExpected_X.Format(path, readKey, key), path, key, readKey);
                            }
                        }
                        else
                        {
                            throw new FileCacheStorageException(Resources.Exception_CacheFile_X_DoesNotContainExpectedKey_X.Format(path, key), path, key);
                        }

                        if (document.RootElement.TryGetProperty(ValuePropertyName, out var valueValue))
                        {
                            value = valueValue.GetBytesFromBase64();
                        }
                        else
                        {
                            throw new FileCacheStorageException(Resources.Exception_CacheFile_X_ForKey_X_DoesNotContainValue.Format(path, key));
                        }

                        if (document.RootElement.TryGetProperty(AbsoluteExpirationPropertyName, out var absoluteExpirationValue))
                        {
                            absoluteExpiration = absoluteExpirationValue.GetDateTimeOffset();
                        }

                        if (document.RootElement.TryGetProperty(SlidingExpirationPropertyName, out var slidingExpirationValue))
                        {
                            // todo: refactor this into non-allocating code if/when the json reader supports timespan or at least writing to a span
                            slidingExpiration = TimeSpan.Parse(slidingExpirationValue.GetString(), CultureInfo.InvariantCulture);
                        }
                    }

                    await stream.DisposeAsync();
                }
            }
            catch (FileNotFoundException)
            {
                // this is okay
                Log.FileNotFound(logger, path, key);
                return null;
            }
            catch (FileCacheStorageException ex)
            {
                // bubble these up
                Log.Failed(logger, path, key, ex);
                throw;
            }
            catch (Exception ex)
            {
                // wrap everything else
                var error = new FileCacheStorageException(Resources.Exception_FailedToReadCacheFile, path, key, ex);
                Log.Failed(logger, path, key, error);
                throw error;
            }

            Log.ReadFile(logger, path, key, value.Length);
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
                var error = new FileCacheStorageException(Resources.Exception_FailedToWriteCacheFile_X_For_Key_X.Format(path, key), path, key, ex);
                Log.Failed(logger, path, key, error);
                throw error;
            }

            Log.WroteFile(logger, path, key, size);
        }

        private static class Log
        {
            #region Failed

            private static readonly Action<ILogger, string, string, Exception> _failed =
                LoggerMessage.Define<string, string>(
                    LogLevel.Error,
                    new EventId(1, nameof(Failed)),
                    Resources.Log_FailedOperationOnCacheFile_X_ForKey_X);

            public static void Failed(ILogger logger, string path, string key, Exception exception) =>
                _failed(logger, path, key, exception);

            #endregion Failed

            #region DeletedFile

            private static readonly Action<ILogger, string, string, Exception> _deletedFileForKey =
                LoggerMessage.Define<string, string>(
                    LogLevel.Debug,
                    new EventId(2, nameof(DeletedFile)),
                    Resources.Log_DeletedCacheFile_X_ForKey_X);

            public static void DeletedFile(ILogger logger, string path, string key) =>
                _deletedFileForKey(logger, path, key, null);

            #endregion DeletedFile

            #region ReadFile

            private static readonly Action<ILogger, string, string, int, Exception> _readFile =
                LoggerMessage.Define<string, string, int>(
                    LogLevel.Debug,
                    new EventId(3, nameof(ReadFile)),
                    Resources.Log_ReadCacheFile_X_ForKey_X_WithValueSizeOf_X_Bytes);

            public static void ReadFile(ILogger logger, string path, string key, int size) =>
                _readFile(logger, path, key, size, null);

            #endregion ReadFile

            #region WroteFile

            private static readonly Action<ILogger, string, string, long, Exception> _wroteFile =
                LoggerMessage.Define<string, string, long>(
                    LogLevel.Debug,
                    new EventId(4, nameof(WroteFile)),
                    Resources.Log_WroteCacheFile_X_ForKey_X_WithValueSizeOf_X_Bytes);

            public static void WroteFile(ILogger logger, string path, string key, long size) =>
                _wroteFile(logger, path, key, size, null);

            #endregion WroteFile

            #region FileNotFound

            private static readonly Action<ILogger, string, string, Exception> _fileNotFound =
                LoggerMessage.Define<string, string>(
                    LogLevel.Debug,
                    new EventId(5, nameof(FileNotFound)),
                    Resources.Log_CacheFile_X_ForKey_X_NotFound);

            public static void FileNotFound(ILogger logger, string path, string key) =>
                _fileNotFound(logger, path, key, null);

            #endregion FileNotFound
        }
    }
}