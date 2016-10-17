using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.App.ViewModels;
using RapidPliant.Lexing.Dfa;

namespace RapidPliant.App.LexDebugger.ViewModels
{
    public class DebuggerLexDfaGraphViewModel : LexDfaGraphViewModel
    {        
        public DebuggerLexDfaGraphViewModel()
        {
            LexPatterns = new ObservableCollection<LexPatternViewModel>();
        }

        public ObservableCollection<LexPatternViewModel> LexPatterns
        {
            get { return get(() => LexPatterns); }
            set { set(()=>LexPatterns, value); }
        }

        protected override void LoadData()
        {
            if (LexPatterns.Any())
            {
                LoadDataForLexRules(
                    LexPatterns.Select(p=> CreateLexRule(p.Pattern, p.Name)).ToArray()
                );
            }
        }

        public void RefreshLexPatterns()
        {
            if (LexPatterns.Any())
            {
                LoadDataForLexRules(
                    LexPatterns.Select(p => CreateLexRule(p.Pattern, p.Name)).ToArray()
                );
            }
        }
    }
}
