using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace RapidPliant.Benchmark
{
    public static class ParseBenchmarkRunner
    {
        public static void Run<TBenchmark>(bool debug = false)
            where TBenchmark : IParseBenchmark, new()
        {
            if (!debug)
            {
                BenchmarkRunner.Run<TBenchmark>();
            }
            else
            {
                var benchmark = new TBenchmark();
                benchmark.Setup();
                benchmark.Run();
            }

            Console.Read();
        }
    }
}
