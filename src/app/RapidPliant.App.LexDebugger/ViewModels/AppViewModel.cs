using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using RapidPliant.App.ViewModels;

namespace RapidPliant.App.LexDebugger.ViewModels
{
    public class AppViewModel : ViewModel
    {
        public DebuggerLexDfaGraphViewModel LexGraphViewModel { get; set; }
        
        public AppViewModel()
        {
            LexPatterns = new ObservableCollection<LexPatternViewModel>();
        }

        public string NewLexPattern
        {
            get { return get(() => NewLexPattern); }
            set { set(() => NewLexPattern, value); }
        }

        public ObservableCollection<LexPatternViewModel> LexPatterns
        {
            get { return get(()=>LexPatterns); }
            set { set(()=>LexPatterns, value); }
        }

        public void AddLexPattern()
        {
            LexPatterns.Add(new LexPatternViewModel("test"));
        }

        protected override void LoadData()
        {
            //Link the lex patterns...
            LexGraphViewModel.LexPatterns = LexPatterns;
        }

        public void AddPattern()
        {
            var lexPatternVm = new LexPatternViewModel(NewLexPattern);
            LexPatterns.Add(lexPatternVm);
            LexGraphViewModel.RefreshLexPatterns();
        }
    }

    public class LexPatternViewModel : ViewModel
    {
        public LexPatternViewModel(string pattern)
        {
            Pattern = pattern;
        }

        public string Name
        {
            get { return get(() => Name); }
            set { set(() => Name, value); }
        }

        public string Pattern
        {
            get { return get(() => Pattern); }
            set { set(()=>Pattern, value); }
        }

        public void Remove()
        {
        }

        public void Refresh()
        {
        }
    }
}
