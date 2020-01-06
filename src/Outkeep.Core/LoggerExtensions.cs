using Microsoft.Extensions.Logging;
using Outkeep.Core.Properties;
using System;

namespace Outkeep.Core
{
    internal static class LoggerExtensions
    {
        #region FileCacheStorage

        #region Failed

        private static readonly Action<ILogger, string, string, Exception> _fileCacheStorageFailed =
            LoggerMessage.Define<string, string>(
                LogLevel.Error,
                new EventId(0, nameof(FileCacheStorageFailed)),
                Resources.Log_FailedOperationOnCacheFile_X_ForKey_X);

        public static void FileCacheStorageFailed(this ILogger logger, string path, string key, Exception exception) =>
            _fileCacheStorageFailed(logger, path, key, exception);

        #endregion Failed

        #region DeletedFile

        private static readonly Action<ILogger, string, string, Exception?> _fileCacheStorageDeletedFileForKey =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(0, nameof(FileCacheStorageDeletedFile)),
                Resources.Log_DeletedCacheFile_X_ForKey_X);

        public static void FileCacheStorageDeletedFile(this ILogger logger, string path, string key) =>
            _fileCacheStorageDeletedFileForKey(logger, path, key, null);

        #endregion DeletedFile

        #region ReadFile

        private static readonly Action<ILogger, string, string, int, Exception?> _fileCacheStorageReadFile =
            LoggerMessage.Define<string, string, int>(
                LogLevel.Debug,
                new EventId(0, nameof(FileCacheStorageReadFile)),
                Resources.Log_ReadCacheFile_X_ForKey_X_WithValueSizeOf_X_Bytes);

        public static void FileCacheStorageReadFile(this ILogger logger, string path, string key, int size) =>
            _fileCacheStorageReadFile(logger, path, key, size, null);

        #endregion ReadFile

        #region WroteFile

        private static readonly Action<ILogger, string, string, long, Exception?> _fileCacheStorageWroteFile =
            LoggerMessage.Define<string, string, long>(
                LogLevel.Debug,
                new EventId(0, nameof(FileCacheStorageWroteFile)),
                Resources.Log_WroteCacheFile_X_ForKey_X_WithValueSizeOf_X_Bytes);

        public static void FileCacheStorageWroteFile(this ILogger logger, string path, string key, long size) =>
            _fileCacheStorageWroteFile(logger, path, key, size, null);

        #endregion WroteFile

        #region FileNotFound

        private static readonly Action<ILogger, string, string, Exception?> _fileCacheStorageFileNotFound =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(0, nameof(FileCacheStorageFileNotFound)),
                Resources.Log_CacheFile_X_ForKey_X_NotFound);

        public static void FileCacheStorageFileNotFound(this ILogger logger, string path, string key) =>
            _fileCacheStorageFileNotFound(logger, path, key, null);

        #endregion FileNotFound

        #endregion FileCacheStorage

        #region CacheDirector

        #region CacheDirectorCompletedOvercapacityCompaction

        private static readonly Action<ILogger, long, Exception?> _cacheDirectorCannotCompactToTargetSisze =
            LoggerMessage.Define<long>(
                LogLevel.Warning,
                new EventId(0, nameof(CacheDirectorCannotCompactToTargetSize)),
                Resources.Log_CacheDirectorCannotCompactToTargetSizeOf_X);

        public static void CacheDirectorCannotCompactToTargetSize(this ILogger logger, long target) =>
            _cacheDirectorCannotCompactToTargetSisze(logger, target, null);

        #endregion CacheDirectorCompletedOvercapacityCompaction

        #endregion CacheDirector
    }
}