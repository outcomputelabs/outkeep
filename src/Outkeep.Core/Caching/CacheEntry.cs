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
    internal sealed class CacheEntry : ICacheEntry, IDisposable
    {
        /// <summary>
        /// The cache context to use for raising notifications.
        /// </summary>
        private readonly ICacheContext _context;

        /// <summary>
        /// Allows users to schedule continuations when the task completes.
        /// </summary>
        private readonly TaskCompletionSource<bool> _evicted;

        public CacheEntry(string key, long size, ICacheContext context)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Size = size < 1 ? throw new ArgumentOutOfRangeException(nameof(size)) : size;

            _context = context ?? throw new ArgumentNullException(nameof(context));

            _evicted = new TaskCompletionSource<bool>();
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

        /// <inheritdoc />
        public Task Evicted => _evicted.Task;

        /// <summary>
        /// Signals users that the parent director has evicted this entry.
        /// </summary>
        public void SetEvicted()
        {
            _evicted.TrySetResult(true);
        }

        /// <inheritdoc />
        public bool TryExpire(DateTimeOffset now)
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
        /// Marks the entry as expired for the given reason, making it ellegible for removal.
        /// </summary>
        internal void SetExpired(EvictionCause reason)
        {
            // the first thread to set an eviction cause wins
            Interlocked.CompareExchange(ref Unsafe.As<EvictionCause, int>(ref _evictionCause), (int)reason, (int)EvictionCause.None);
        }

        /// <inheritdoc />
        public ICacheEntry Commit()
        {
            EnsureUncommitted();

            _committed = true;

            _context.OnEntryCommitted(this);

            return this;
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

        #region Disposable

        private bool _disposed;

        /// <summary>
        /// Disposing the cache entry removes it from the owning cache director.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            SetExpired(EvictionCause.Disposed);
            _context.OnEntryExpired(this);

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~CacheEntry()
        {
            Dispose();
        }

        #endregion Disposable
    }
}