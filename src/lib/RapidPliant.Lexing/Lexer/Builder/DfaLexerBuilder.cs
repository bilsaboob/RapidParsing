using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Lexing.Automata.Dfa;
using RapidPliant.Lexing.Automata.Nfa;
using RapidPliant.Lexing.Pattern.Regex;

namespace RapidPliant.Lexing.Lexer.Builder
{
    public class DfaLexerBuilder
    {
        private RapidRegex _regex;

        private List<RegexExpr> _lexExpressions;

        public DfaLexerBuilder()
        {
            _regex = new RapidRegex();
            _lexExpressions = new List<RegexExpr>();
        }

        public DfaLexerBuilder Pattern(string pattern, string name)
        {
            var lexExpr = CreateLexExpr(pattern, name);
            _lexExpressions.Add(lexExpr);
            return this;
        }

        public Lexer CreateLexer()
        {
            var lexDfa = CreateLexDfa(_lexExpressions);
            var recognizer = new LexDfaTableTransitionRecognizer(lexDfa);
            var tokenizer = new SingleCaptureDfaTokenizer(recognizer);
            return new Lexer(tokenizer);
        }

        #region Nfa / Dfa helpers

        protected RegexExpr CreateLexExpr(string regexPattern, string name = null)
        {
            var expr = _regex.FromPattern(regexPattern);
            if (!string.IsNullOrEmpty(name))
                expr.Name = name;
            return expr;
        }

        protected DfaGraph CreateLexDfa(params RegexExpr[] expressions)
        {
            return CreateLexDfa(expressions.AsEnumerable());
        }

        protected DfaGraph CreateLexDfa(IEnumerable<RegexExpr> expressions)
        {
            var nfa = LexNfaAutomata.BuildNfa(expressions);
            var nfaGraph = nfa.ToNfaGraph();

            var dfa = LexDfaAutomata.BuildDfa(nfaGraph);
            return dfa.ToDfaGraph();
        }

        #endregion
    }

    public static class DfaLexerBuilderExtensions
    {
        public static DfaLexerBuilder Identifier(this DfaLexerBuilder b, string name)
        {
            b.Pattern("([a-z]|[A-Z]|_)([a-z]|[A-Z]|_|[0-9])*+", name);
            return b;
        }

        public static DfaLexerBuilder IntegerLiteral(this DfaLexerBuilder b, string name)
        {
            b.Pattern("([0-9])+", name);
            return b;
        }

        public static DfaLexerBuilder BoolLiteral(this DfaLexerBuilder b, string name)
        {
            b.Pattern("([0-9])+", name);
            return b;
        }

        public static DfaLexerBuilder StringLiteral(this DfaLexerBuilder b, string name)
        {
            b.Pattern("\"(\\.)*\"", name);
            return b;
        }

        public static DfaLexerBuilder CharStringLiteral(this DfaLexerBuilder b, string name)
        {
            b.Pattern("'(\\.)*'", name);
            return b;
        }

        public static DfaLexerBuilder CharLiteral(this DfaLexerBuilder b, string name)
        {
            b.Pattern("'(\\.)'", name);
            return b;
        }

        public static DfaLexerBuilder False(this DfaLexerBuilder b, string name)
        {
            b.Pattern("([0-9])+", name);
            return b;
        }
    }

    public class TestBuilder
    {
        public void Test()
        {
            var b = new DfaLexerBuilder();

            // Always start with the ones that are dynamic
            b.Identifier("IDENTIFIER");
            b.IntegerLiteral("INTEGER");

            // Finish with overrides - the keywords and finally symbols
            b.Pattern("public", "PUBLIC");
            b.Pattern("static", "STATIC");

            b.Pattern("{", "LB");
            b.Pattern("}", "RB");

            var lexer = b.CreateLexer();

            // finish with the symbols, always highest priority

            //etc...
        }
    }
}
