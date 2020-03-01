using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Benchmarks
{
    public class ThrowingExtensionsInliningBenchmarks
    {
        private readonly object _target = new object();
        private readonly Consumer _consumer = new Consumer();

        [Benchmark]
        public void GetTypeNameWithoutCheck()
        {
            _consumer.Consume(_target.CallGetTypeNameWithoutCheck());
        }

        [Benchmark(Baseline = true)]
        public void GetTypeNameWithCheck()
        {
            _consumer.Consume(_target.CallGetTypeNameWithCheck());
        }

        [Benchmark]
        public void GetTypeNameWithFunctionCheck()
        {
            _consumer.Consume(_target.CallGetTypeNameWithFunctionCheck());
        }
    }

    public static class ParentExtensions
    {
        public static string CallGetTypeNameWithoutCheck(this object target)
        {
            return target.GetTypeNameWithoutCheck();
        }

        public static string CallGetTypeNameWithCheck(this object target)
        {
            return target.GetTypeNameWithCheck();
        }

        public static string CallGetTypeNameWithFunctionCheck(this object target)
        {
            return target.GetTypeNameWithFunctionCheck();
        }
    }

    public static class InliningExtensions
    {
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        public static string GetTypeNameWithoutCheck(this object target)
        {
            return target.GetType().FullName;
        }

        public static string GetTypeNameWithCheck(this object target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return target.GetType().FullName;
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        [SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced")]
        public static string GetTypeNameWithFunctionCheck(this object target)
        {
            if (target == null) ThrowTargetNull();

            return target.GetType().FullName;

            static void ThrowTargetNull() => throw new ArgumentNullException(nameof(target));
        }
    }
}