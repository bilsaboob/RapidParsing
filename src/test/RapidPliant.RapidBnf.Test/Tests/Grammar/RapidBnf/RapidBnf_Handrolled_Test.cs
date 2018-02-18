using System;

namespace RapidPliant.RapidBnf.Test.Tests.Grammar
{
    public class RapidBnf_Handrolled_Test : RapidBnf_Handrolled_ParseTestBase
    {
        protected override void Benchmark()
        {
            Parse(Text_Rbnf_1000_Lines);
        }
        
        protected override void Test()
        {
            ParseRepeated(Text_Rbnf_1000_Lines, 100);
            ParseUntilUserInput(Text_Rbnf_1000_Lines);
        }

        #region test input
        //        private static readonly string grammarInputText = @"
        //RuleA = 132.5345 RuleB | 'some' RuleC /a test | some/ /another (testing here)/;
        //RuleB = 'test';
        //RuleC = RuleA;
        //";

        public static readonly string grammarInputText = @"
//This is a line comment
/* this is a 
multi line 
cool pattern */
RuleA = (group one)?. | (group two)* /a test | some/ .4 /another (testing here)/;
";
        #endregion
    }
}
