using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace RapidPliant.Benchmark.LargeJsonFile
{
    class Program
    {
        static void Main(string[] args)
        {
            var debug = true;
            ParseBenchmarkRunner.Run<LargeJsonFileBenchmark>(debug);
        }
    }
}
