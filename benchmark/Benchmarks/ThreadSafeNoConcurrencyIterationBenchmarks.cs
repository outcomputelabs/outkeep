using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Benchmarks
{
    [Orderer(SummaryOrderPolicy.SlowestToFastest)]
    [MemoryDiagnoser, GcServer(true), GcConcurrent(true), GcForce(true)]
    public class LowConcurrencyThreadSafeLoopingBenchmarks
    {
        private ConcurrentDictionary<object, byte> _concurrentDictionary;
        private Dictionary<object, byte> _dictionary;
        private List<object> _list;
        private ImmutableList<object> _ilist;
        private ImmutableArray<object> _iarray;
        private CopyingCollection<object> _larray;
        private object[] _array;

        private readonly object _lock = new object();
        private readonly Consumer _consumer = new Consumer();

        [Params(10)]
        public int N { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _concurrentDictionary = new ConcurrentDictionary<object, byte>(Environment.ProcessorCount, N);
            _dictionary = new Dictionary<object, byte>(N);
            _list = new List<object>(N);
            _array = new object[N];
            _larray = new CopyingCollection<object>();

            var immutableListBuilder = ImmutableList.CreateBuilder<object>();
            var immutableArrayBuilder = ImmutableArray.CreateBuilder<object>(N);
            for (var i = 0; i < N; ++i)
            {
                var obj = new object();

                _concurrentDictionary[obj] = 0x00;
                _dictionary[obj] = 0x00;
                _list.Add(obj);
                _array[i] = obj;
                _larray.Add(obj);
                immutableListBuilder.Add(obj);
                immutableArrayBuilder.Add(obj);
            }

            _ilist = immutableListBuilder.ToImmutable();
            _iarray = immutableArrayBuilder.MoveToImmutable();
        }

        [Benchmark]
        public void ForEachListItemNoLocking()
        {
            foreach (var item in _list)
            {
                _consumer.Consume(item);
            }
        }

        [Benchmark]
        public void ForListIndexNoLocking()
        {
            for (var i = 0; i < _list.Count; ++i)
            {
                _consumer.Consume(_list[i]);
            }
        }

        [Benchmark]
        public void ForEachConcurrentDictionaryKeyValuePairs()
        {
            foreach (var pair in _concurrentDictionary)
            {
                _consumer.Consume(pair);
            }
        }

        [Benchmark]
        public void ForEachConcurrentDictionaryKeys()
        {
            foreach (var key in _concurrentDictionary.Keys)
            {
                _consumer.Consume(key);
            }
        }

        [Benchmark]
        public void ForEachDictionaryKeyValuePairsWithLock()
        {
            lock (_lock)
            {
                foreach (var pair in _dictionary)
                {
                    _consumer.Consume(pair);
                }
            }
        }

        [Benchmark]
        public void ForEachDictionaryKeysWithLock()
        {
            lock (_lock)
            {
                foreach (var key in _dictionary.Keys)
                {
                    _consumer.Consume(key);
                }
            }
        }

        [Benchmark]
        public void ForEachListItemWithLock()
        {
            lock (_lock)
            {
                foreach (var item in _list)
                {
                    _consumer.Consume(item);
                }
            }
        }

        [Benchmark]
        public void ForListIndexWithLock()
        {
            lock (_lock)
            {
                for (var i = 0; i < _list.Count; ++i)
                {
                    _consumer.Consume(_list[i]);
                }
            }
        }

        [Benchmark]
        public void ForEachImmutableListItem()
        {
            foreach (var item in _ilist)
            {
                _consumer.Consume(item);
            }
        }

        [Benchmark]
        public void ForImmutableListIndex()
        {
            for (var i = 0; i < _ilist.Count; ++i)
            {
                _consumer.Consume(_ilist[i]);
            }
        }

        [Benchmark]
        public void ForImmutableArrayIndex()
        {
            for (var i = 0; i < _iarray.Length; ++i)
            {
                _consumer.Consume(_iarray[i]);
            }
        }

        [Benchmark(Baseline = true)]
        public void ForArrayIndexWithNoLock()
        {
            for (var i = 0; i < _array.Length; ++i)
            {
                _consumer.Consume(_array[i]);
            }
        }

        [Benchmark]
        public void ForArrayIndexWithLock()
        {
            lock (_lock)
            {
                for (var i = 0; i < _array.Length; ++i)
                {
                    _consumer.Consume(_array[i]);
                }
            }
        }
    }
}