using AntlrTest;
using RapidPliant.Benchmark;
using RapidPliant.RapidBnf.Test.Tests.Grammar;

namespace RapidPliant.RapidBnf.Test.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var debug = false;
#if DEBUG
            debug = true;
#endif

            RunBenchmark(debug);
        }

        private static void RunBenchmark(bool debug)
        {
            //BenchmarkRunner.Run<RapidBnf_Handrolled_Benchmark>(debug);

            // Antlr
            //BenchmarkRunner.Run<RapidBnf_Antlr_Benchmark>(debug);
            BenchmarkRunner.Run<RapidSharp_Antlr_Benchmark>(debug);
        }
    }

    public class RapidBnf_Handrolled_Benchmark : TestBenchmark<RapidBnf_Handrolled_Test> {}

    #region Antlr
    public class RapidBnf_Antlr_Benchmark : TestBenchmark<RapidBnf_Antlr_Test> { }
    public class RapidSharp_Antlr_Benchmark : TestBenchmark<RapidSharp_Antlr_Test> { }
    #endregion
}
