using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using RazorEngineCore;
using RazorEngine = RazorEngineCore.RazorEngine;

namespace DotchatBenchmark;

[CPUUsageDiagnoser]
public class Benchmarks
{
    
}