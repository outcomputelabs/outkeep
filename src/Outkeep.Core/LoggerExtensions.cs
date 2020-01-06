using Microsoft.Extensions.Logging;
using Outkeep.Core.Properties;
using System;

namespace Outkeep.Core
{
    internal static class LoggerExtensions
    {
        #region FileCacheStorage

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
    }
}