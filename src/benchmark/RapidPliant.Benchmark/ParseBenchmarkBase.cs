using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace RapidPliant.Benchmark
{
    public abstract class ParseBenchmarkBase : IParseBenchmark
    {
        protected object Grammar { get; set; }
        protected object Parser { get; set; }
        protected object Engine { get; set; }
        protected TextReader Input { get; set; }

        [Setup]
        public void Setup()
        {
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

        protected abstract object CreateGrammar();
        protected abstract object CreateEngine();
        protected abstract object CreateParser();
        protected abstract TextReader GetInput();
    }
}
