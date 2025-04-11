using BenchmarkDotNet.Running;
using Conduit.Benchmark.Benchmarks;

namespace Conduit.Benchmark;

public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<RequestBenchmarks>();
        //BenchmarkRunner.Run<NotificationBenchmarks>();
    }
}