using System.Collections.ObjectModel;
using System.Linq;
using RapidPliant.App.ViewModels;
using RapidPliant.Grammar;
using RapidPliant.Parsing.Earley;

namespace RapidPliant.App.GrammarDebugger.ViewModels
{
    public class DebuggerEarleyDfaGraphViewModel : EarleyDfaGraphViewModel
    {
        public DebuggerEarleyDfaGraphViewModel()
        {
        }

        public EarleyGrammar EarleyGrammar
        {
            get { return get(() => EarleyGrammar); }
            set { set(() => EarleyGrammar, value); }
        }

        protected override void LoadData()
        {
            if (EarleyGrammar != null)
            {
                LoadDataForGrammar(EarleyGrammar);
            }
        }

        public void RefreshGrammar()
        {
            if (EarleyGrammar != null)
            {
                LoadDataForGrammar(EarleyGrammar);
            }
        }
    }
}