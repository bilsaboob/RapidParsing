using RapidPliant.RapidBnf.Test.Tests.Grammar;
using RapidPliant.Test;

namespace RapidPliant.RapidBnf.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRunner.Run<RapidBnf_Handrolled_Test>();
            
            //TestRunner.Run<RapidBnfHandrolledHandrolledTest>();
            //TestRunner.Run<RegexPatternTest>();
            //TestRunner.Run<NonOverlappingSetTest>();
            //TestRunner.Run<SimpleGrammarTest>();
        }
    }
}
