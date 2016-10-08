using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Lexing.Dfa;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Lexing.Pattern.Regex;
using RapidPliant.Test;

namespace RapidPliant.Testing.Tests
{
    public class RegexPatternTest : TestBase
    {
        private LexDfaFactory LexDfaFactory = new LexDfaFactory();
        private RapidRegex Regex = new RapidRegex();

        protected override void Test()
        {
            RegexPatternExpr p;
            LexDfaTableLexer l;
            LexDfaState d;
            string s;

            p = Regex.FromPattern("a|(bc|i|k)");
            s = p.ToString();
            
            //Create the dfa!
            l = CreateLexer("a|(bc|i|k)");

            TestLexing("a|(bc|i|k)", "ai");

            TestLexing(l, "ai");
            TestLexing(l, "a");

            /*p = Regex.FromPattern("a|bc|d|efg");
            s = p.ToString();

            p = Regex.FromPattern("a|bcijk|dzde|efg|ooo");
            s = p.ToString();*/

        }

        private void TestLexing(string regexPattern, string input)
        {
            var lexer = CreateLexer(regexPattern);
            TestLexing(lexer, input);
        }

        private void TestLexing(LexDfaTableLexer lexer, string input)
        {
            for (var i = 0; i < input.Length; ++i)
            {
                if(!lexer.CanContinue)
                    break;

                var ch = input[i];

                lexer.Scan(ch);

                if (lexer.ScannedCapturesCount > 0)
                {
                    //We have new captures
                    foreach (var scannedCapture in lexer.ScannedCaptures)
                    {
                        var speling = scannedCapture.Spelling;
                    }
                }
            }

            if (lexer.CapturesCount > 0)
            {
                //Check all the captures again!
                foreach (var capture in lexer.Captures)
                {
                    var spelling = capture.Spelling;
                }
            }
        }

        private LexDfaTableLexer CreateLexer(string regexPattern)
        {
            var patternExpr = Regex.FromPattern(regexPattern);
            var dfa = CreateLexDfa(patternExpr);
            return new LexDfaTableLexer(dfa);
        }

        private LexDfaTableLexer CreateLexer(RegexPatternExpr patternExpr)
        {
            var dfa = CreateLexDfa(patternExpr);
            return new LexDfaTableLexer(dfa);
        }

        private LexDfaState CreateLexDfa(RegexPatternExpr patternExpr)
        {
            //Build a rule from the pattern expression
            var patternRule = new LexPatternRule();
            patternRule.FromExpression(patternExpr);

            //Build dfa for the rule
            var dfaState = LexDfaFactory.BuildFromLexPatternRule(patternRule);
            return dfaState;
        }
    }
}
