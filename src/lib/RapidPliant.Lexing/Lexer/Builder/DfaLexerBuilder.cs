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

        public DfaLexerBuilder Pattern(string pattern, object tag, string name = null)
        {
            var lexExpr = CreateLexExpr(pattern, tag, name);
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

        protected RegexExpr CreateLexExpr(string regexPattern, object tag, string name = null)
        {
            var expr = _regex.FromPattern(regexPattern);
            expr.Tag = tag;

            if (string.IsNullOrEmpty(name))
                name = tag.ToString();

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
        public static DfaLexerBuilder Identifier(this DfaLexerBuilder b, object tag, string name = null)
        {
            b.Pattern("([a-z]|[A-Z]|_)([a-z]|[A-Z]|_|[0-9])*", tag, name);
            return b;
        }

        public static DfaLexerBuilder IntegerLiteral(this DfaLexerBuilder b, object tag, string name = null)
        {
            b.Pattern("([0-9])+", tag, name);
            return b;
        }

        public static DfaLexerBuilder BoolLiteral(this DfaLexerBuilder b, object tag, string name = null)
        {
            b.Pattern("([0-9])+", tag, name);
            return b;
        }

        public static DfaLexerBuilder StringLiteral(this DfaLexerBuilder b, object tag, string name = null)
        {
            b.Pattern("\"(\\.)*\"", tag, name);
            return b;
        }

        public static DfaLexerBuilder CharStringLiteral(this DfaLexerBuilder b, object tag, string name = null)
        {
            b.Pattern("'\\.*'", tag, name);
            return b;
        }

        public static DfaLexerBuilder CharLiteral(this DfaLexerBuilder b, object tag, string name = null)
        {
            b.Pattern("'(\\.)'", tag, name);
            return b;
        }

        public static DfaLexerBuilder False(this DfaLexerBuilder b, object tag, string name = null)
        {
            b.Pattern("([0-9])+", tag, name);
            return b;
        }
    }
}
