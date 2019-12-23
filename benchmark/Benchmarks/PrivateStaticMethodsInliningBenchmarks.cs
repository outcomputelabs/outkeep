using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Benchmarks
{
    public class PrivateMemberMethodsInliningBenchmarks
    {
        private readonly object _target = new object();
        private readonly Consumer _consumer = new Consumer();

        [Benchmark]
        public void GetTypeNameWithoutCheck()
        {
            _consumer.Consume(DoCallGetTypeNameWithoutCheck());
        }

        [Benchmark(Baseline = true)]
        public void GetTypeNameWithCheck()
        {
            _consumer.Consume(DoCallGetTypeNameWithCheck());
        }

        [Benchmark]
        public void GetTypeNameWithFunctionCheck()
        {
            _consumer.Consume(DoCallGetTypeNameWithFunctionCheck());
        }

        private string DoCallGetTypeNameWithoutCheck()
        {
            return DoGetTypeNameWithoutCheck();
        }

        private string DoCallGetTypeNameWithCheck()
        {
            return DoGetTypeNameWithCheck();
        }

        private string DoCallGetTypeNameWithFunctionCheck()
        {
            return DoGetTypeNameWithFunctionCheck();
        }

        private string DoGetTypeNameWithoutCheck()
        {
            return _target.GetType().FullName;
        }

        private string DoGetTypeNameWithCheck()
        {
            if (_target == null) throw new InvalidOperationException();

            return _target.GetType().FullName;
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        private string DoGetTypeNameWithFunctionCheck()
        {
            if (_target == null) ThrowTargetNull();

            return _target.GetType().FullName;

            static void ThrowTargetNull() => throw new InvalidOperationException();
        }
    }
}