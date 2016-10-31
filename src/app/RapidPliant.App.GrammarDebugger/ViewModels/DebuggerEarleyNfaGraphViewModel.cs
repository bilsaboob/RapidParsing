using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.App.ViewModels;
using RapidPliant.Grammar;
using RapidPliant.Parsing.Earley;

namespace RapidPliant.App.GrammarDebugger.ViewModels
{
    public class DebuggerEarleyNfaGraphViewModel : EarleyNfaGraphViewModel
    {
        public DebuggerEarleyNfaGraphViewModel()
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
