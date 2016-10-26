using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Lexing.Pattern.Regex;
using RapidPliant.Test;
using RapidPliant.Util;

namespace RapidPliant.Testing.Tests
{
    public class RegexPatternTest : TestBase
    {
        private RapidRegex Regex = new RapidRegex();

        protected override void Test()
        {
            var nfaBuilder = new NfaBuilder();
            var dfaBuilder = new DfaBuilder();

            //var expr = CreateLexExpr("a(bc|bd)*e");
            var expr = CreateLexExpr("a([b-e])*bekj");

            var nfa = nfaBuilder.Create(expr);
            var nfaGraph = nfa.ToNfaGraph();

            var dfa = dfaBuilder.Create(nfa);
            var dfaGraph = dfa.ToDfaGraph();
            
            TestLexing(dfaGraph, "abbbbbeeeeecccccddddeeeeebekj");
        }

        private void TestLexing(DfaGraph dfaGraph, string input)
        {
            var lexer = new DfaLexer(dfaGraph);
            var inputReader = new StringReader(input);

            IReadOnlyList<SpellingCapture> captures = null;
            while (lexer.CanContinue)
            {
                var i = inputReader.Read();
                if(i == -1)
                    break;

                var ch = (char) i;

                lexer.Scan(ch);

                //Check the captures that were captured for this scan pass
                captures = lexer.ScannedCaptures;
                if (captures != null && captures.Count > 0)
                {
                    //There can be multiple captures with same spelling, in case of multiple rules that match the same input
                    foreach (var capture in captures)
                    {
                        var spelling = capture.Spelling;
                    }
                }
            }

            //Check all the captures
            captures = lexer.Captures;
            if (captures != null && captures.Count > 0)
            {
                foreach (var capture in captures)
                {
                    var spelling = capture.Spelling;
                }
            }
        }

        protected RegexExpr CreateLexExpr(string regexPattern)
        {
            return Regex.FromPattern(regexPattern);
        }
    }
}
