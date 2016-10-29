using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using RapidPliant.Common.Rule;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Lexing.Pattern.Regex;

namespace RapidPliant.App.ViewModels
{
    public class LexNfaGraphViewModel : LexGraphViewModel
    {
        private LexNfaMsaglGraphModel GraphModel { get; set; }

        public LexNfaGraphViewModel()
        {
            GraphModel = new LexNfaMsaglGraphModel();
        }

        protected virtual void LoadDataForLexExpressions(params RegexExpr[] lexExpressions)
        {
            var nfa = CreateLexNfaGraph(lexExpressions);
            LoadDataForLexGraph(nfa);
        }

        protected virtual void LoadDataForLexPattern(string lexPattern)
        {
            var nfa = CreateLexNfaGraph(lexPattern);
            LoadDataForLexGraph(nfa);
        }

        protected virtual void LoadDataForLexGraph(LexNfaAutomata.LexNfaGraph nfa)
        {
            //Iterate the lex def and create a graph!
            var states = nfa.States.OrderBy(s => s.Id).ToList();

            //Build the graph model for the states
            GraphModel.Build(states);

            //Set the graph!
            Graph = GraphModel.Graph;
        }
    }
}
