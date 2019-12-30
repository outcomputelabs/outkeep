using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;

namespace Benchmarks
{
    [ShortRunJob]
    [MemoryDiagnoser, GcServer(true), GcConcurrent(true), GcForce(true), RunOncePerIteration]
    public class ConcurrentEnumeration
    {
        private ConcurrentDictionary<string, Item> _dictionary;
        private ConcurrentQueue<Item> _queue;
        private ConcurrentBag<Item> _bag;

        [Params(100000)]
        public int N { get; set; }

        [Params(10)]
        public int Remove { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _dictionary = new ConcurrentDictionary<string, Item>(Environment.ProcessorCount, N);
            _queue = new ConcurrentQueue<Item>();
            _bag = new ConcurrentBag<Item>();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            while (_dictionary.Count < N)
            {
                var item = new Item
                {
                    Key = Guid.NewGuid().ToString()
                };
                _dictionary.TryAdd(item.Key, item);
            }

            while (_queue.Count < N)
            {
                _queue.Enqueue(new Item { Key = Guid.NewGuid().ToString() });
            }

            while (_bag.Count < N)
            {
                _bag.Add(new Item { Key = Guid.NewGuid().ToString() });
            }
        }

        [Benchmark(Baseline = true)]
        public void ConcurrentDictionaryValueEnumeration()
        {
            var i = 0;
            foreach (var item in _dictionary.Values)
            {
                if (i++ % Remove == 0)
                {
                    _dictionary.TryRemove(item.Key, out _);
                }
            }
        }

        [Benchmark]
        public void ConcurrentDictionaryEntriesEnumeration()
        {
            var i = 0;
            foreach (var item in _dictionary)
            {
                if (i++ % Remove == 0)
                {
                    _dictionary.TryRemove(item.Key, out _);
                }
            }
        }

        [Benchmark]
        public void ConcurrentQueue()
        {
            var count = _queue.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_queue.TryDequeue(out var entry))
                {
                    if (i % Remove == 0)
                    {
                        // let it go
                    }
                    else
                    {
                        _queue.Enqueue(entry);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        [Benchmark]
        public void ConcurrentBag()
        {
            var count = _bag.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_bag.TryTake(out var entry))
                {
                    if (i % Remove == 0)
                    {
                        // let it go
                    }
                    else
                    {
                        _bag.Add(entry);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    internal class Item
    {
        public string Key { get; set; }
    }
}