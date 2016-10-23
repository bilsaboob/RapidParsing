using RapidPliant.Lexing.Graph;
using RapidPliant.Lexing.Pattern.Regex;

namespace RapidPliant.App.ViewModels
{
    public class LexGraphViewModel : GraphViewModel
    {
        public LexGraphViewModel()
        {
            LexNfaBuilder = new NfaBuilder();
            LexDfaBuilder = new DfaBuilder();
            Regex = new RapidRegex();
        }

        protected NfaBuilder LexNfaBuilder { get; set; }
        protected DfaBuilder LexDfaBuilder { get; set; }
        protected RapidRegex Regex { get; set; }

        #region Helpers
        protected RegexExpr CreateLexExpr(string regexPattern, string name = null)
        {
            var expr = Regex.FromPattern(regexPattern);

            if (!string.IsNullOrEmpty(name))
                expr.Name = name;

            return expr;
        }

        protected NfaGraph CreateLexNfaGraph(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern, name);
            return CreateLexNfaGraph(patternExpr, name);
        }

        protected DfaGraph CreateLexDfaGraph(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern, name);
            return CreateLexDfaGraph(patternExpr, name);
        }

        protected NfaGraph CreateLexNfaGraph(params RegexExpr[] patternExpressions)
        {
            return LexNfaBuilder.Create(patternExpressions);
        }

        protected DfaGraph CreateLexDfaGraph(params RegexExpr[] patternExpressions)
        {
            var nfa = LexNfaBuilder.Create(patternExpressions);
            var dfa = LexDfaBuilder.Create(nfa.Nfa);
            return dfa;
        }

        protected NfaGraph CreateLexNfaGraph(RegexExpr patternExpr, string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                patternExpr.Name = name;
            }

            return LexNfaBuilder.Create(patternExpr);
        }

        protected DfaGraph CreateLexDfaGraph(RegexExpr patternExpr, string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                patternExpr.Name = name;
            }

            var nfa = LexNfaBuilder.Create(patternExpr);
            var dfa = LexDfaBuilder.Create(nfa.Nfa);
            return dfa;
        }
        #endregion
    }
}