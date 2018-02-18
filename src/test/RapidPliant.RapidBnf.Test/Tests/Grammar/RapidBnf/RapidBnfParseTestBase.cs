using System;
using System.Diagnostics;
using RapidPliant.Test;

namespace RapidPliant.RapidBnf.Test.Tests.Grammar {
    public class RapidBnfParseTestBase : TestBase
    {
        public string Text_Rbnf_1000_Lines { get; set; }
        public string Text_Rbnf_Grammar { get; set; }

        protected override void SetupTest()
        {
            base.SetupTest();

            TestFilesPath = "testfiles\\RapidBnf";

            Text_Rbnf_1000_Lines = ReadTestFile("1000_lines.txt");
            Text_Rbnf_Grammar = ReadTestFile("RbnfGrammar.txt");
        }

        protected void PrintStats(ParseStats stats, int count = 1)
        {
            Console.WriteLine(stats.Print(count));
        }

        protected class ParseStats
        {
            private Stopwatch sw = new Stopwatch();

            public long LexTime { get; set; }
            public long ParseTime { get; set; }

            public long TotalTime => LexTime + ParseTime;

            public override string ToString()
            {
                return $"{TotalTime.ToString().PadLeft(3)} ({LexTime}/{ParseTime})";
            }

            public void Start()
            {
                sw.Restart();
            }

            public void Lexed()
            {
                sw.Stop();
                LexTime += sw.ElapsedMilliseconds;
                sw.Restart();
            }

            public void Parsed()
            {
                sw.Stop();
                ParseTime += sw.ElapsedMilliseconds;
                sw.Restart();
            }

            public void End()
            {
                sw.Stop();
            }

            public string Print(int count = 1)
            {
                return $"{(TotalTime / count).ToString().PadLeft(3)} ({(LexTime / count)}/{(ParseTime / count)})";
            }
        }
    }
}