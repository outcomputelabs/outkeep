using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Outkeep.Core.Caching;
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
        private readonly CacheOptions _options;
        private readonly TimerArgs _timerArgs;

        public CacheDirectorGrainService(IGrainIdentity identity, Silo silo, ILoggerFactory loggerFactory, ILogger<CacheDirectorGrainService> logger, IOptions<CacheOptions> options, ICacheDirector<string> director)
            : base(identity, silo, loggerFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            if (director is null) throw new ArgumentNullException(nameof(director));

            _timerArgs = new TimerArgs(this, director, _logger);
        }

        private IDisposable? _evictionTimer;

        public override Task Start()
        {
            // schedule removal of expired items
            _evictionTimer = RegisterTimer(
                TickEvictExpired,
                _timerArgs,
                _options.ExpirationScanFrequency,
                _options.ExpirationScanFrequency);

            Log.CacheDirectorGrainServiceStarted(_logger);

            return base.Start();
        }

        public override Task Stop()
        {
            _evictionTimer?.Dispose();

            Log.CacheDirectorGrainServiceStopped(_logger);

            return base.Stop();
        }

        private static Task TickEvictExpired(object state)
        {
            var args = (TimerArgs)state;

            try
            {
                args.Director.EvictExpired();
            }
            catch (Exception ex)
            {
                Log.CacheDirectorGrainServiceError(args.Logger, ex);
                throw;
            }

            return Task.CompletedTask;
        }

        public Task PingAsync() => Task.CompletedTask;

        /// <summary>
        /// Groups state information for timer callbacks.
        /// </summary>
        private class TimerArgs
        {
            public TimerArgs(CacheDirectorGrainService service, ICacheDirector<string> director, ILogger logger)
            {
                Service = service;
                Director = director;
                Logger = logger;
            }

            public CacheDirectorGrainService Service { get; }
            public ICacheDirector<string> Director { get; }
            public ILogger Logger { get; }
        }

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