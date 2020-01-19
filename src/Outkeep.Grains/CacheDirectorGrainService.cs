using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Outkeep.Grains.Properties;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    /// <summary>
    /// Default implementation of <see cref="ICacheDirectorGrainService"/>.
    /// </summary>
    [Reentrant]
    public class CacheDirectorGrainService : GrainService, ICacheDirectorGrainService
    {
        private readonly ILogger _logger;

        public CacheDirectorGrainService(IGrainIdentity identity, Silo silo, ILoggerFactory loggerFactory, ILogger<CacheDirectorGrainService> logger)
            : base(identity, silo, loggerFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task Start()
        {
            Log.CacheDirectorGrainServiceStarted(_logger);

            return base.Start();
        }

        public override Task Stop()
        {
            Log.CacheDirectorGrainServiceStopped(_logger);

            return base.Stop();
        }

        public Task PingAsync() => Task.CompletedTask;

        private static class Log
        {
            public static void CacheDirectorGrainServiceStarted(ILogger logger) => _cacheDirectorGrainServiceStarted(logger, null);

            private static readonly Action<ILogger, Exception?> _cacheDirectorGrainServiceStarted = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1, nameof(CacheDirectorGrainServiceStarted)),
                Resources.Log_CacheDirectorGrainServiceStarted);

            public static void CacheDirectorGrainServiceStopped(ILogger logger) => _cacheDirectorGrainServiceStopped(logger, null);

            private static readonly Action<ILogger, Exception?> _cacheDirectorGrainServiceStopped = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(2, nameof(CacheDirectorGrainServiceStopped)),
                Resources.Log_CacheDirectorGrainServiceStopped);

            public static void CacheDirectorGrainServiceError(ILogger logger, Exception exception) => _cacheDirectorGrainServiceError(logger, exception);

            private static readonly Action<ILogger, Exception> _cacheDirectorGrainServiceError = LoggerMessage.Define(
                LogLevel.Error,
                new EventId(3, nameof(CacheDirectorGrainServiceError)),
                Resources.Log_CachedDirectorGrainServiceError);
        }
    }
}