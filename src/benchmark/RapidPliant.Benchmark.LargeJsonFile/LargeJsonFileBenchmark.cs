using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RapidPliant.Grammar;
using RapidPliant.Runtime.Earley.Grammar;

namespace RapidPliant.Benchmark.LargeJsonFile
{
    public class LargeJsonFileBenchmark : ParseBenchmarkBase
    {
        protected override IGrammarModel CreateGrammarModel()
        {
            var g = new SimpleJsonGrammarModel();
            g.Build();
            return g;
        }

        protected override IEarleyGrammar CreateGrammar()
        {
            var g = new EarleyGrammar(GrammarModel);
            g.Compile();
            return g;
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
