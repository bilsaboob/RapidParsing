using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Lexing.Graph;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Lexing.Pattern.Regex;
using RapidPliant.Test;

namespace RapidPliant.Testing.Tests
{
    public class RegexPatternTest : TestBase
    {
        private RapidRegex Regex = new RapidRegex();

        protected override void Test()
        {
            var nfaFactory = new NfaBuilder();
            //var expr = CreateLexExpr("a(bc|bd)e");
            var expr = CreateLexExpr("abc");
            var nfaGraph = nfaFactory.Create(expr);
            var dfaGraph = nfaGraph.Nfa.ToDfa();
        }

        protected RegexExpr CreateLexExpr(string regexPattern)
        {
            return Regex.FromPattern(regexPattern);
        }
    }
}
