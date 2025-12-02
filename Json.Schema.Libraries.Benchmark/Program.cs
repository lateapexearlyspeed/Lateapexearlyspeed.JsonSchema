using BenchmarkDotNet.Running;

namespace Json.Schema.Libraries.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            var summary = BenchmarkRunner.Run<BenchmarkTests>();

            // var bigDataTests = new BigDataBenchmarkTests();
            // bigDataTests.ValidateByJsonSchemaDotNet();
            // bigDataTests.ValidateByLateApexEarlySpeed();
        }
    }
}
