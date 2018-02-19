using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Test
{
    public abstract class TestBase : ITest
    {
        protected TestBase()
        {
        }

        public bool IsBenchmark { get; private set; }

        public string TestFilesPath { get; protected set; }
        
        public void Setup(bool benchmark = false)
        {
            IsBenchmark = benchmark;

            TestFilesPath = "testfiles";

            if (benchmark)
            {
                SetupBenchmark();
            }
            else
            {
                SetupTest();
            }
        }

        protected virtual void SetupBenchmark()
        {
            SetupTest();
        }

        protected virtual void SetupTest()
        {
        }

        public bool Run(bool benchmark = false)
        {
            IsBenchmark = benchmark;

            if (benchmark)
            {
                Benchmark();
            }
            else
            {
                Test();
            }

            return true;
        }

        protected virtual void Benchmark()
        {
            Test();
        }

        protected virtual void Test()
        {
            //Do the parsing
        }

        protected string ReadTestFile(string name)
        {
            return File.ReadAllText(Path.Combine(TestFilesPath, name));
        }

        protected void GarbageCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
