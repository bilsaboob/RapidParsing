using Microsoft.Msagl.Drawing;
using RapidPliant.Automata.Dfa;
using RapidPliant.Grammar;
using RapidPliant.Lexing.Automata;
using RapidPliant.Lexing.Automata.Dfa;
using RapidPliant.Parsing.Earley;
using RapidPliant.Util;

namespace RapidPliant.App.ViewModels
{
    public class EarleyDfaMsaglGraphModel : DfaMsaglGraphModel
    {
        public EarleyDfaMsaglGraphModel()
        {
        }

        public EarleyGrammar EarleyGrammar { get; set; }

        protected override string GetTransitionLabel(IDfaTransition transition)
        {
            var grammarDefId = (int)transition.TransitionValue;
            var idLabel = $"{grammarDefId}";

            var defNameLabel = "";
            if (grammarDefId > 0)
            {
                var grammarDef = EarleyGrammar.GrammarModel.GetDefById(grammarDefId);
                if (grammarDef != null)
                {
                    defNameLabel = $"{grammarDef.Name}";
                }
            }
            
            return $"{idLabel}:{defNameLabel}";
        }
    }
}