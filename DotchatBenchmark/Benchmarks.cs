using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace DotchatBenchmark;

[CPUUsageDiagnoser]
public class Benchmarks
{
    [GlobalSetup]
    public void Setup() { }
}