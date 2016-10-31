using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Dfa;
using RapidPliant.Grammar;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Pattern.Regex;
using RapidPliant.Parsing.Earley;

namespace RapidPliant.App.ViewModels
{
    public class EarleyDfaGraphViewModel : EarleyGraphViewModel
    {
        private EarleyDfaMsaglGraphModel GraphModel { get; set; }
        private EarleyGrammar EarleyGrammar { get; set; }

        public EarleyDfaGraphViewModel()
        {
            GraphModel = new EarleyDfaMsaglGraphModel();
        }

        protected void LoadDataForGrammar(EarleyGrammar earleyGrammar)
        {
            //Build the earley grammar for the specified grammar model
            EarleyGrammar = earleyGrammar;
            EarleyGrammar.EnsureBuild();

            //Set the earley grammar in the graph model
            GraphModel.EarleyGrammar = EarleyGrammar;

            LoadDataForEarleyGraph(EarleyGrammar.EarleyDfa);
        }

        protected virtual void LoadDataForEarleyGraph(IDfaGraph dfa)
        {
            if(GraphModel == null)
                throw new Exception("A grammar must be specified!");

            var states = dfa.States.OrderBy(s => s.Id).ToList();
            GraphModel.Build(states);
            Graph = GraphModel.Graph;
        }
    }
}
