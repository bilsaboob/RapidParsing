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
            TestLexing("peterstora",
                CreateLexRule("pet", "pet1"),
                CreateLexRule("pet", "pet2"),
                CreateLexRule("petersto", "petersto2")
            );

            /*TestLexing(
                "abefg",
                "ab|abd|abefg|ijkl"
            );*/

            /*RegexPatternExpr p;
            LexDfaTableLexer l;
            LexDfaState d;
            string s;
            
            var r6 = CreateLexDfa(
                CreateLexRule("a|b|c", "A"),
                CreateLexRule("d|e|f", "B")
            );

            var r = CreateLexDfa("ab|cd|efgh|ijklmn", "A");

            //var r2 = CreateLexRule("(a|b)(c|d)");
            //var r3 = CreateLexRule("(ab|cd)(ef|gh)");
            //var r4 = CreateLexRule("(a|c)(e|g)(i|j)");

            /*p = Regex.FromPattern("a|(bc|i|k)");
            s = p.ToString();
            
            //Create the dfa!
            l = CreateLexer("a|(bc|i|k)");

            TestLexing("a|(bc|i|k)", "ai");

            TestLexing(l, "ai");
            TestLexing(l, "a");*/

            /*p = Regex.FromPattern("a|bc|d|efg");
            s = p.ToString();

            p = Regex.FromPattern("a|bcijk|dzde|efg|ooo");
            s = p.ToString();*/

        }
        
        private void TestLexing(string input, string regexPattern)
        {
            var lexer = CreateLexer(regexPattern);
            TestLexing(lexer, input);
        }

        private void TestLexing(string input, params LexPatternRule[] patternRules)
        {
            var dfa = CreateLexDfa(patternRules);
            var lexer = new LexDfaTableLexer(dfa);
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

        protected LexDfaTableLexer CreateLexer(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern);
            var dfa = CreateLexDfa(patternExpr, name);
            return new LexDfaTableLexer(dfa);
        }
        
        protected LexDfaTableLexer CreateLexer(RegexPatternExpr patternExpr, string name = null)
        {
            var dfa = CreateLexDfa(patternExpr, name);
            return new LexDfaTableLexer(dfa);
        }

        protected RegexPatternExpr CreateLexExpr(string regexPattern)
        {
            return Regex.FromPattern(regexPattern);
        }

        protected LexDfa CreateLexDfa(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern);
            return CreateLexDfa(patternExpr, name);
        }

        protected LexPatternRule CreateLexRule(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern);
            //Build a rule from the pattern expression
            var patternRule = new LexPatternRule(name);
            patternRule.FromExpression(patternExpr);
            return patternRule;
        }

        protected LexDfa CreateLexDfa(params LexPatternRule[] lexRules)
        {
            //Build dfa for the rule
            var dfa = LexDfaFactory.BuildFromLexPatternRules(lexRules);
            return dfa;
        }

        protected LexDfa CreateLexDfa(RegexPatternExpr patternExpr, string name = null)
        {
            //Build a rule from the pattern expression
            var patternRule = new LexPatternRule(name);
            patternRule.FromExpression(patternExpr);

            //Build dfa for the rule
            var dfa = LexDfaFactory.BuildFromLexPatternRule(patternRule);
            return dfa;
        }
    }
}
