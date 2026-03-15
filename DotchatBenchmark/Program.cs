using BenchmarkDotNet.Running;

namespace DotchatBenchmark;

internal class Program
{
    static void Main(string[] args)
    {
        var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
