using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Pattern.Regex;

namespace RapidPliant.App.ViewModels
{
    public class LexDfaGraphViewModel : LexGraphViewModel
    {
        private LexDfaMsaglGraphModel GraphModel { get; set; }

        public LexDfaGraphViewModel()
        {
            GraphModel = new LexDfaMsaglGraphModel();
        }

        protected virtual void LoadDataForLexExpressions(params RegexExpr[] lexExpressions)
        {
            var dfa = CreateLexDfaGraph(lexExpressions);
            LoadDataForLexGraph(dfa);
        }

        protected virtual void LoadDataForLexPattern(string lexPattern)
        {
            var dfa = CreateLexDfaGraph(lexPattern);
            LoadDataForLexGraph(dfa);
        }

        protected virtual void LoadDataForLexGraph(IDfaGraph dfa)
        {
            //Iterate the lex def and create a graph!
            var states = dfa.States.OrderBy(s => s.Id).ToList();

            //Build the graph model for the states
            GraphModel.Build(states);

            //Set the graph!
            Graph = GraphModel.Graph;
        }
    }
}
