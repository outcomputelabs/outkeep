using Outkeep.Core.Properties;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Implements a cache entry as used by <see cref="CacheDirector"/>.
    /// </summary>
    internal sealed class CacheEntry : ICacheEntry, ICacheEntryContext, IDisposable
    {
        /// <summary>
        /// The cache context to use for raising notifications.
        /// </summary>
        private readonly ICacheContext _context;

        /// <summary>
        /// The user expiry callback registration to invoke upon expiry.
        /// </summary>
        private PostEvictionCallbackRegistration? _postEvictionCallbackRegistration;

        public CacheEntry(string key, long size, ICacheContext context)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Size = size < 1 ? throw new ArgumentOutOfRangeException(nameof(size)) : size;

            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Whether this entry has been added to the owner cache director.
        /// </summary>
        private bool _committed;

        /// <summary>
        /// The cause behind the eviction of this entry.
        /// </summary>
        private EvictionCause _evictionCause = EvictionCause.None;

        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public long Size { get; }

        /// <inheritdoc />
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <inheritdoc />
        public TimeSpan? SlidingExpiration { get; set; }

        /// <inheritdoc />
        public DateTimeOffset UtcLastAccessed { get; set; }

        /// <inheritdoc />
        public CachePriority Priority { get; set; } = CachePriority.Normal;

        /// <inheritdoc />
        public EvictionCause EvictionCause => _evictionCause;

        /// <inheritdoc />
        public bool IsExpired => _evictionCause != EvictionCause.None;

        /// <summary>
        /// Marks the entry as expired if it has reached expiration time thresholds.
        /// </summary>
        internal bool TrySetExpiredOnTimeout(DateTimeOffset now)
        {
            // no-op if the entry is already expired
            if (IsExpired) return true;

            // expire if the entry reached absolute expiration time regardless of sliding expiration time
            if (AbsoluteExpiration.HasValue && AbsoluteExpiration.Value <= now)
            {
                SetExpired(EvictionCause.Expired);
                return true;
            }

            // otherwise expire if the entry reached sliding expiration
            if (SlidingExpiration.HasValue && now - UtcLastAccessed >= SlidingExpiration)
            {
                SetExpired(EvictionCause.Expired);
                return true;
            }

            // otherwise dont expire
            return false;
        }

        /// <summary>
        /// Schedules the user supplied eviction callback for execution.
        /// This method is for use by the parent director upon actual eviction from the cache.
        /// </summary>
        internal void ScheduleEvictionCallback()
        {
            // swap the registration to ensure we only ever call it once and release its resources early
            var registration = Interlocked.Exchange(ref _postEvictionCallbackRegistration, null);

            // no-op if no registration to invoke
            if (registration == null) return;

            // schedule the callback to run on the same scheduler where the entry was created
            // this will normally be an orleans grain activation task scheduler
            Task.Factory.StartNew(
                registration.Callback,
                registration.State,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                registration.TaskScheduler);
        }

        /// <summary>
        /// Marks the entry as expired for the given reason, making it ellegible for removal.
        /// </summary>
        internal void SetExpired(EvictionCause reason)
        {
            EnsureCommitted();

            // the first thread to set an eviction cause wins
            Interlocked.CompareExchange(ref Unsafe.As<EvictionCause, int>(ref _evictionCause), (int)reason, (int)EvictionCause.None);
        }

        /// <inheritdoc />
        public IDisposable SetPostEvictionCallback(Action<object?> callback, object? state, TaskScheduler taskScheduler)
        {
            EnsureUncommitted();

            if (callback == null) throw new ArgumentNullException(nameof(callback));

            // create a new registration
            var registration = new PostEvictionCallbackRegistration(callback, state, taskScheduler, this);

            // swap with any existing registration and early dispose it
            Interlocked.Exchange(ref _postEvictionCallbackRegistration, registration)?.Dispose();

            // return the registration created by this thread
            // a race condition is possible here if a concurrent thread disposes the current registration before this one returns it
            // this is okay as registration disposal is reentrant and thread-safe
            return registration;
        }

        /// <inheritdoc />
        public void Commit()
        {
            EnsureUncommitted();

            _committed = true;

            _context.OnEntryCommitted(this);
        }

        /// <inheritdoc />
        public void Expire()
        {
            EnsureCommitted();

            SetExpired(EvictionCause.UserExpired);

            _context.OnEntryExpired(this);
        }

        /// <summary>
        /// Ensures the entry is uncommitted.
        /// Protects methods that require the entry to be uncomitted.
        /// </summary>
        /// <exception cref="InvalidOperationException">The entry is committed.</exception>
        private void EnsureUncommitted()
        {
            if (!_committed) return;

            throw new InvalidOperationException(Resources.Exception_CacheEntryFor_X_IsCommittedAndDoesNotAllowThisOperation.Format(Key));
        }

        /// <summary>
        /// Ensures the entry is committed.
        /// Protects methods that require the entry to be committed.
        /// </summary>
        /// <exception cref="InvalidOperationException">The entry is uncommitted.</exception>
        private void EnsureCommitted()
        {
            if (_committed) return;

            throw new InvalidOperationException(Resources.Exception_CacheEntryFor_X_IsUncommittedAndDoesNotAllowThisOperation.Format(Key));
        }

        /// <summary>
        /// Called by the post eviction callback registration upon disposal.
        /// </summary>
        /// <param name="registration">The registration that was disposed.</param>
        public void OnPostEvictionCallbackRegistrationDisposed(PostEvictionCallbackRegistration registration)
        {
            // remove the registration if it is the current one
            // otherwise it was already removed
            Interlocked.CompareExchange(ref _postEvictionCallbackRegistration, null, registration);
        }

        /// <summary>
        /// Disposing the cache entry removes it from the owning cache director.
        /// </summary>
        public void Dispose()
        {
            // set the cause for eviction
            SetExpired(EvictionCause.Disposed);

            // notify the owning context tnhat this entry has expired
            _context.OnEntryExpired(this);
        }
    }
}