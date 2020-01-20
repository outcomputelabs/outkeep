using Outkeep.Core.Properties;
using System;
using System.Threading;

namespace Outkeep.Core.Caching
{
    /// <summary>
    /// Implements a cache entry as used by <see cref="CacheDirector"/>.
    /// </summary>
    internal sealed class CacheEntry<TKey> : ICacheEntry<TKey> where TKey : notnull
    {
        /// <summary>
        /// The cache context to use for raising notifications.
        /// </summary>
        private readonly ICacheContext<TKey> _context;

        /// <summary>
        /// Allows users to schedule continuations when the task completes.
        /// </summary>
        private readonly CancellationTokenSource _revokedSource = new CancellationTokenSource();

        public CacheEntry(TKey key, long size, ICacheContext<TKey> context)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Size = size < 1 ? throw new ArgumentOutOfRangeException(nameof(size)) : size;

            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Whether this entry has been added to the owner cache director.
        /// </summary>
        private bool _committed;

        /// <inheritdoc />
        public TKey Key { get; }

        /// <inheritdoc />
        public long Size { get; }

        /// <inheritdoc />
        public CachePriority Priority { get; set; } = CachePriority.Normal;

        /// <inheritdoc />
        public CancellationToken Revoked => _revokedSource.Token;

        /// <inheritdoc />
        public bool IsRevoked => _revokedSource.IsCancellationRequested;

        /// <inheritdoc />
        public ICacheEntry<TKey> Commit()
        {
            EnsureUncommitted();

            _committed = true;

            _context.OnEntryCommitted(this);

            return this;
        }

        /// <inheritdoc />
        public void Revoke()
        {
            EnsureCommitted();

            _revokedSource.Cancel();
            _context.OnEntryRevoked(this);
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
        /// Disposing the allocation entry also revokes it.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (!_revokedSource.IsCancellationRequested)
            {
                _revokedSource.Cancel();
                _context.OnEntryRevoked(this);
            }
            _revokedSource.Dispose();

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