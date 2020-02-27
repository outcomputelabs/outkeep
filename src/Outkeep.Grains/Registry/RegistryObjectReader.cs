using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Registry
{
    // todo: remove the new constraint and replace the type creation with the serializer call
    internal class RegistryObjectReader<TState> : IEnumerable<TState> where TState : new()
    {
        private Enumerator? _enumerator;

        public RegistryObjectReader(DbDataReader reader)
        {
            _enumerator = new Enumerator(reader);
        }

        public IEnumerator<TState> GetEnumerator()
        {
            var enumerator = Interlocked.Exchange(ref _enumerator, null);

            if (enumerator is null)
                throw new InvalidOperationException("Cannot enumerate more than once.");

            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<TState>, IAsyncEnumerator<TState>, IDisposable, IAsyncDisposable
        {
            private readonly DbDataReader _reader;
            private readonly FieldInfo[] _fields;
            private int[] _lookup;

            public Enumerator(DbDataReader reader)
            {
                _reader = reader;
                _fields = typeof(TState).GetFields();
            }

            private TState _current;

            public TState Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose()
            {
                _reader.Dispose();
            }

            public bool MoveNext()
            {
                if (_reader.Read())
                {
                    if (_lookup == null)
                    {
                        InitFieldLookup();
                    }

                    // todo: remove this and replace with a serializer call
                    var instance = new TState();

                    for (var i = 0; i < _lookup.Length; ++i)
                    {
                        var index = _lookup[i];
                        if (index >= 0)
                        {
                            var field = _fields[i];
                            if (_reader.IsDBNull(index))
                            {
                                field.SetValue(instance, null);
                            }
                            else
                            {
                                field.SetValue(instance, _reader.GetValue(index));
                            }
                        }
                    }

                    _current = instance;

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void InitFieldLookup()
            {
                var map = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
                for (int i = 0, n = _reader.FieldCount; i < n; i++)
                {
                    map.Add(_reader.GetName(i), i);
                }

                _lookup = new int[_fields.Length];

                for (int i = 0, n = _fields.Length; i < n; i++)
                {
                    if (map.TryGetValue(_fields[i].Name, out var index))
                    {
                        _lookup[i] = index;
                    }
                    else
                    {
                        _lookup[i] = -1;
                    }
                }
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                if (await _reader.ReadAsync().ConfigureAwait(false))
                {
                    if (_lookup == null)
                    {
                        InitFieldLookup();
                    }

                    // todo: remove this and replace with a serializer call
                    var instance = new TState();

                    for (var i = 0; i < _lookup.Length; ++i)
                    {
                        var index = _lookup[i];
                        if (index >= 0)
                        {
                            var field = _fields[i];
                            if (_reader.IsDBNull(index))
                            {
                                field.SetValue(instance, null);
                            }
                            else
                            {
                                field.SetValue(instance, _reader.GetValue(index));
                            }
                        }
                    }

                    _current = instance;

                    return true;
                }

                return false;
            }

            public ValueTask DisposeAsync()
            {
                return _reader.DisposeAsync();
            }
        }
    }
}