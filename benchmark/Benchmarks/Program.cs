using BenchmarkDotNet.Running;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage()]

namespace Benchmarks
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}