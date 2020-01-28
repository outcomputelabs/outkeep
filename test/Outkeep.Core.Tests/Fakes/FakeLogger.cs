using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Outkeep.Core.Tests.Fakes
{
    public class FakeLogger<T> : ILogger<T>
    {
        private int _scope = 0;

        public IList<string> Output { get; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            _scope++;
            return new DisposableAction(() => _scope--);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter is null) throw new ArgumentNullException(nameof(formatter));

            Output.Add(formatter.Invoke(state, exception));
        }

        private sealed class DisposableAction : IDisposable
        {
            private readonly Action _action;

            public DisposableAction(Action action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }

            public void Dispose()
            {
                _action();
            }
        }
    }
}