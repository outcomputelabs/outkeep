using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Benchmarks
{
    public class PrivateStaticMethodsInliningBenchmarks
    {
        private readonly object _target = new object();
        private readonly Consumer _consumer = new Consumer();

        [Benchmark]
        public void GetTypeNameWithoutCheck()
        {
            _consumer.Consume(DoCallGetTypeNameWithoutCheck(_target));
        }

        [Benchmark(Baseline = true)]
        public void GetTypeNameWithCheck()
        {
            _consumer.Consume(DoCallGetTypeNameWithCheck(_target));
        }

        [Benchmark]
        public void GetTypeNameWithFunctionCheck()
        {
            _consumer.Consume(DoCallGetTypeNameWithFunctionCheck(_target));
        }

        private static string DoCallGetTypeNameWithoutCheck(object target)
        {
            return DoGetTypeNameWithoutCheck(target);
        }

        private static string DoCallGetTypeNameWithCheck(object target)
        {
            return DoGetTypeNameWithCheck(target);
        }

        private static string DoCallGetTypeNameWithFunctionCheck(object target)
        {
            return DoGetTypeNameWithFunctionCheck(target);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        private static string DoGetTypeNameWithoutCheck(object target)
        {
            return target.GetType().FullName;
        }

        private static string DoGetTypeNameWithCheck(object target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return target.GetType().FullName;
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        [SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced")]
        private static string DoGetTypeNameWithFunctionCheck(object target)
        {
            if (target == null) ThrowTargetNull();

            return target.GetType().FullName;

            static void ThrowTargetNull() => throw new ArgumentNullException(nameof(target));
        }
    }
}