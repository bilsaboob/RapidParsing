using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace RapidPliant.Benchmark.LargeJsonFile
{
    public class LargeJsonFileBenchmark : ParseBenchmarkBase
    {
        protected override object CreateGrammar()
        {
            return null;
        }

        protected override object CreateEngine()
        {
            return null;
        }

        protected override object CreateParser()
        {
            return null;
        }

        protected override TextReader GetInput()
        {
            return null;
        }
    }
}
