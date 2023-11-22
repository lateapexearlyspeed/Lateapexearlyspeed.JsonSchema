using BenchmarkDotNet.Running;

namespace LateApexEarlySpeed.Json.Schema.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkTestClass>();
        }
    }
}