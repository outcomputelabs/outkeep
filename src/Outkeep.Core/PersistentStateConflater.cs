using Orleans.Runtime;
using Outkeep.Properties;
using System;
using System.Threading.Tasks;

namespace Outkeep
{
    /// <summary>
    /// A rudimentary wrapper that batches reads and writes to the orleans state in a reentrancy safe manner.
    /// </summary>
    internal class PersistentStateConflater<TState> : IPersistentState<TState>
        where TState : new()
    {
        public PersistentStateConflater(IPersistentState<TState> underlying)
        {
            _underlying = underlying ?? throw new ArgumentNullException(nameof(underlying));
        }

        private readonly IPersistentState<TState> _underlying;
        private Task? _outstanding = null;

        public TState State
        {
            get => _underlying.State;
            set => _underlying.State = value;
        }

        public string Etag => _underlying.Etag;

        public Task ClearStateAsync()
        {
            return BatchStateAsync(OperationType.Clear);
        }

        public Task ReadStateAsync()
        {
            return BatchStateAsync(OperationType.Read);
        }

        public Task WriteStateAsync()
        {
            return BatchStateAsync(OperationType.Write);
        }

        private async Task BatchStateAsync(OperationType type)
        {
            // check if there is an outstanding storage operation
            // if there is one it will not include the values we have just staged
            var thisTask = _outstanding;
            if (thisTask != null)
            {
                try
                {
                    await thisTask.ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    // while we do not want to observe past exceptions
                    // we do want to stop execution early if the outstanding operation failed
                    throw new InvalidOperationException(Resources.Exception_CannotExecuteStorageOperationBecauseThePriorOperationFailed, ex);
                }
                finally
                {
                    // if this continuation beat other write attempts then we null out the task to allow further attempts
                    // if the tasks are different then other attempts have already started
                    if (thisTask == _outstanding)
                    {
                        _outstanding = null;
                    }
                }
            }

            // at this point this continuation is either the first to get here or other continuations may have gotten here already
            if (_outstanding == null)
            {
                // this means this is (probably) the first continuation to get to this point
                // therefore this continuation gets to start the next storage operation
                _outstanding = type switch
                {
                    OperationType.Read => _underlying.ReadStateAsync(),
                    OperationType.Write => _underlying.WriteStateAsync(),
                    OperationType.Clear => _underlying.ClearStateAsync(),
                    _ => throw new InvalidOperationException(),
                };
                thisTask = _outstanding;
            }
            else
            {
                // getting here means another continuation has already started a write operation
                // that write operation already covers the value of the current task so we can await on it
                thisTask = _outstanding;
            }

            // either way we can now wait for the outstanding operation
            try
            {
                await thisTask.ConfigureAwait(true);
            }
            finally
            {
                // if this continuation beat other write attempts here then we null out the task to allow further attempts
                // otherwise some other write attempt has already started
                if (thisTask == _outstanding)
                {
                    _outstanding = null;
                }
            }
        }

        private enum OperationType
        {
            Read,
            Write,
            Clear
        }
    }
}