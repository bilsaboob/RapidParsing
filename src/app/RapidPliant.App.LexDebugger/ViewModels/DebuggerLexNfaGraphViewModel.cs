using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.App.ViewModels;

namespace RapidPliant.App.LexDebugger.ViewModels
{
    public class DebuggerLexNfaGraphViewModel : LexNfaGraphViewModel
    {        
        public DebuggerLexNfaGraphViewModel()
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
                LoadDataForLexExpressions(
                    LexPatterns.Select(p=> CreateLexExpr(p.Pattern, p.Name)).ToArray()
                );
            }
        }

        public void RefreshLexPatterns()
        {
            if (LexPatterns.Any())
            {
                LoadDataForLexExpressions(
                    LexPatterns.Select(p => CreateLexExpr(p.Pattern, p.Name)).ToArray()
                );
            }
        }
    }
}
