using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using RapidPliant.Automata.Dfa;
using RapidPliant.Automata.Nfa;
using RapidPliant.Common.Rule;
using RapidPliant.Grammar;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Lexing.Pattern.Regex;
using RapidPliant.Parsing.Earley;

namespace RapidPliant.App.ViewModels
{
    public class EarleyNfaGraphViewModel : EarleyGraphViewModel
    {
        private EarleyNfaMsaglGraphModel GraphModel { get; set; }
        private EarleyGrammar EarleyGrammar { get; set; }

        public EarleyNfaGraphViewModel()
        {
            GraphModel = new EarleyNfaMsaglGraphModel();
        }

        protected void LoadDataForGrammar(EarleyGrammar earleyGrammar)
        {
            //Build the earley grammar for the specified grammar model
            EarleyGrammar = earleyGrammar;
            EarleyGrammar.EnsureBuild();

            LoadDataForEarleyGraph(EarleyGrammar.EarleyNfa);
        }

        protected virtual void LoadDataForEarleyGraph(INfaGraph nfa)
        {
            if (GraphModel == null)
                throw new Exception("A grammar must be specified!");

            var states = nfa.States.OrderBy(s => s.Id).ToList();
            GraphModel.Build(states);
            Graph = GraphModel.Graph;
        }
    }
}
