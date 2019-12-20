using System;
using System.Threading;

namespace Outkeep.Core
{
    public sealed class DisposableAction : IDisposable
    {
        private Action<object?> _action;
        private readonly object? _state;

        public DisposableAction(Action<object?> action, object? state)
        {
            _action = action;
            _state = state;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _action, null!)?.Invoke(_state);
        }
    }
}