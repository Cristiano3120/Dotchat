using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace DotchatBenchmark;

internal static class Program
{
    static void Main()
    {
        Summary[] _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}