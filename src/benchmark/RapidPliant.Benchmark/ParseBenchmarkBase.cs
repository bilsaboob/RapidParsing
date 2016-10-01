using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using RapidPliant.Grammar;
using RapidPliant.Runtime.Earley.Grammar;

namespace RapidPliant.Benchmark
{
    public abstract class ParseBenchmarkBase : IParseBenchmark
    {
        protected IGrammarModel GrammarModel { get; set; }
        protected object Grammar { get; set; }
        protected object Parser { get; set; }
        protected object Engine { get; set; }
        protected TextReader Input { get; set; }

        [Setup]
        public void Setup()
        {
            GrammarModel = CreateGrammarModel();
            Grammar = CreateGrammar();
        }
        
        [Benchmark]
        public bool Run()
        {
            Engine = CreateEngine();
            Parser = CreateParser();
            Input = GetInput();

            Parse();

            return true;
        }
        
        protected virtual void Parse()
        {
            //Do the parsing
        }

        protected abstract IGrammarModel CreateGrammarModel();
        protected abstract IEarleyGrammar CreateGrammar();

        protected abstract object CreateEngine();
        protected abstract object CreateParser();
        protected abstract TextReader GetInput();
    }
}
