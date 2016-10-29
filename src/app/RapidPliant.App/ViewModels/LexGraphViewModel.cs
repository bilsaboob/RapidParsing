using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Pattern.Regex;

namespace RapidPliant.App.ViewModels
{
    public class LexGraphViewModel : GraphViewModel
    {
        public LexGraphViewModel()
        {
            Regex = new RapidRegex();
        }

        protected RapidRegex Regex { get; set; }

        #region Helpers
        protected RegexExpr CreateLexExpr(string regexPattern, string name = null)
        {
            var expr = Regex.FromPattern(regexPattern);

            if (!string.IsNullOrEmpty(name))
                expr.Name = name;

            return expr;
        }

        protected LexNfaAutomata.LexNfaGraph CreateLexNfaGraph(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern, name);
            return CreateLexNfaGraph(patternExpr, name);
        }

        protected DfaGraph CreateLexDfaGraph(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern, name);
            return CreateLexDfaGraph(patternExpr, name);
        }

        protected LexNfaAutomata.LexNfaGraph CreateLexNfaGraph(params RegexExpr[] patternExpressions)
        {
            var nfa = LexNfaAutomata.BuildNfa(patternExpressions);
            return (LexNfaAutomata.LexNfaGraph)nfa.ToNfaGraph();
        }

        protected DfaGraph CreateLexDfaGraph(params RegexExpr[] patternExpressions)
        {
            var nfa = LexNfaAutomata.BuildNfa(patternExpressions);
            var nfaGraph = nfa.ToNfaGraph();
            var dfa = LexDfaAutomata.BuildDfa(nfaGraph);
            return dfa.ToDfaGraph();
        }

        protected LexNfaAutomata.LexNfaGraph CreateLexNfaGraph(RegexExpr patternExpr, string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                patternExpr.Name = name;
            }

            var nfa = LexNfaAutomata.BuildNfa(patternExpr);
            return (LexNfaAutomata.LexNfaGraph)nfa.ToNfaGraph();
        }

        protected DfaGraph CreateLexDfaGraph(RegexExpr patternExpr, string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                patternExpr.Name = name;
            }

            var nfa = LexNfaAutomata.BuildNfa(patternExpr);
            var nfaGraph = nfa.ToNfaGraph();
            var dfa = LexDfaAutomata.BuildDfa(nfaGraph);
            return dfa.ToDfaGraph();
        }
        #endregion
    }
}