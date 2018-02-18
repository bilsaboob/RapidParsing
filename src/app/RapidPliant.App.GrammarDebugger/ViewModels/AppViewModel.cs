using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using RapidPliant.App.ViewModels;
using RapidPliant.Grammar;
using RapidPliant.Parsing.Earley;
using RapidPliant.RapidBnf.Test.Tests.Grammar;
using RapidPliant.WPF.Mvx;

namespace RapidPliant.App.GrammarDebugger.ViewModels
{
    public class AppViewModel : ViewModel
    {
        public AppViewModel()
        {
            GrammarModel = new SimpleGrammarModel();
        }

        public EarleyGrammar EarleyGrammar { get; private set; }

        public IGrammarModel GrammarModel
        {
            get { return get(() => GrammarModel); }
            set
            {
                //Create the new earley grammar
                if (EarleyGrammar == null || EarleyGrammar.GrammarModel != value)
                {
                    EarleyGrammar = new EarleyGrammar(value);
                }

                set(() => GrammarModel, value);
            }
        }

        public DebuggerEarleyNfaGraphViewModel EarleyNfaGraph
        {
            get { return get(() => EarleyNfaGraph); }
            set { set(() => EarleyNfaGraph, value); }
        }

        public DebuggerEarleyDfaGraphViewModel EarleyDfaGraph
        {
            get { return get(() => EarleyDfaGraph); }
            set { set(() => EarleyDfaGraph, value); }
        }

        protected override void LoadData()
        {
            if (EarleyNfaGraph != null)
            {
                EarleyNfaGraph.EarleyGrammar = EarleyGrammar;
            }

            if (EarleyDfaGraph != null)
            {
                EarleyDfaGraph.EarleyGrammar = EarleyGrammar;
            }
            
            RefreshGrammarForGraphs();
        }

        private void RefreshGrammarForGraphs()
        {
            //Refresh both the nfa/dfa
            EarleyNfaGraph.RefreshGrammar();
            EarleyDfaGraph.RefreshGrammar();
        }
    }
}
