using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Core;
using Orleans.Runtime;
using Outkeep.Core.Caching;
using Outkeep.Grains.Properties;
using Outkeep.Interfaces;
using System;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    public class CacheDirectorGrainService : GrainService, ICacheDirectorGrainService
    {
        private readonly ILogger<CacheDirectorGrainService> _logger;
        private readonly CacheDirectorOptions _options;
        private readonly ICacheDirector _director;
        private readonly TimerArgs _timerArgs;

        public CacheDirectorGrainService(IGrainIdentity identity, Silo silo, ILoggerFactory loggerFactory, ILogger<CacheDirectorGrainService> logger, IOptions<CacheDirectorOptions> options, ICacheDirector director)
            : base(identity, silo, loggerFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _director = director ?? throw new ArgumentNullException(nameof(director));

            _timerArgs = new TimerArgs(_director, _logger);
        }

        private IDisposable _removeExpiredTimer;
        private IDisposable _overcapacityRegistration;

        public override Task Start()
        {
            _removeExpiredTimer = RegisterTimer(
                TickRemoveExpired,
                new TimerArgs(_director, _logger),
                _options.ExpirationScanFrequency,
                _options.ExpirationScanFrequency);

            _overcapacityRegistration = _director.RegisterOvercapacityCallback(
                state =>
                {
                    // todo: issue compaction here after
                    // the objective is to run compaction on an orleans worker thread and avoid unnecessary noise on the thread pool
                },
                this,
                TaskScheduler.Current);

            Log.CacheDirectorGrainServiceStarted(_logger);

            return base.Start();
        }

        public override Task Stop()
        {
            Log.CacheDirectorGrainServiceStopped(_logger);

            return base.Stop();
        }

        private static Task TickRemoveExpired(object state)
        {
            var args = (TimerArgs)state;

            try
            {
                args.Director.RemoveExpired();
            }
            catch (Exception ex)
            {
                Log.CacheDirectorGrainServiceError(args.Logger, ex);
                throw;
            }

            return Task.CompletedTask;
        }

        public Task PingAsync() => Task.CompletedTask;

        private class TimerArgs
        {
            public TimerArgs(ICacheDirector director, ILogger<CacheDirectorGrainService> logger)
            {
                Director = director;
                Logger = logger;
            }

            public ICacheDirector Director { get; }
            public ILogger<CacheDirectorGrainService> Logger { get; }
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