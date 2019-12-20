using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Benchmarks
{
    public class LockingImmutableArray<T> : ICollection<T>
    {
        private readonly object _lock = new object();
        private ImmutableArray<T> _array = ImmutableArray<T>.Empty;

        public int Count => _array.Length;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            lock (_lock)
            {
                _array = _array.Add(item);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _array = ImmutableArray<T>.Empty;
            }
        }

        public bool Contains(T item) => _array.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _array.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            lock (_lock)
            {
                var replaced = _array;
                _array = _array.Remove(item);
                return _array != replaced;
            }
        }

        public ImmutableArray<T>.Enumerator GetEnumerator() => _array.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_array).GetEnumerator();
    }
}