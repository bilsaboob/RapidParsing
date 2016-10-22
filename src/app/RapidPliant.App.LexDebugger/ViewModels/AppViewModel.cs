using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using RapidPliant.App.ViewModels;
using RapidPliant.WPF.Mvx;

namespace RapidPliant.App.LexDebugger.ViewModels
{
    public class AppViewModel : ViewModel
    {
        private List<string> _nextPatternAutoNames;

        public DebuggerLexDfaGraphViewModel LexGraphViewModel { get; set; }
        
        public AppViewModel()
        {
            LexPatterns = new ObservableCollection<LexPatternViewModel>();
            _nextPatternAutoNames = new List<string>();

            CreatePatternNames();

            ConsumeNextPatternName();
        }

        private void ConsumeNextPatternName()
        {
            NewLexPatternName = _nextPatternAutoNames.First();
            _nextPatternAutoNames.RemoveAt(0);
            NewLexPatternIsAutoName = true;
        }

        private void CreatePatternNames()
        {
            for (var i = 'A'; i < 'Z'; ++i)
            {
                _nextPatternAutoNames.Add(i.ToString());
            }
        }

        public string NewLexPatternName
        {
            get { return get(() => NewLexPatternName); }
            set
            {
                if (!_nextPatternAutoNames.Contains(value))
                {
                    if (NewLexPatternIsAutoName)
                    {
                        var prevVal = get(() => NewLexPatternName);
                        if (!string.IsNullOrEmpty(prevVal) && !_nextPatternAutoNames.Contains(prevVal))
                        {
                            _nextPatternAutoNames.Insert(0, prevVal);
                        }
                    }
                    NewLexPatternIsAutoName = false;
                }

                set(() => NewLexPatternName, value);
            }
        }

        public bool NewLexPatternIsAutoName { get; set; }

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

        protected override void LoadData()
        {
            //Link the lex patterns...
            LexGraphViewModel.LexPatterns = LexPatterns;
        }

        public void AddPattern()
        {
            var lexPatternVm = new LexPatternViewModel(NewLexPattern, NewLexPatternName);
            lexPatternVm.IsAutoName = NewLexPatternIsAutoName;
            _nextPatternAutoNames.Remove(NewLexPattern);

            ConsumeNextPatternName();

            LexPatterns.Add(lexPatternVm);
            LexGraphViewModel.RefreshLexPatterns();

            NewLexPattern = "";
        }

        public void RemovePattern(LexPatternViewModel lexPattern)
        {
            LexPatterns.Remove(lexPattern);
            LexGraphViewModel.RefreshLexPatterns();

            if (NewLexPatternIsAutoName && !string.IsNullOrEmpty(NewLexPatternName))
            {
                if (!_nextPatternAutoNames.Contains(NewLexPatternName))
                {
                    _nextPatternAutoNames.Insert(0, NewLexPatternName);
                }
            }

            if (lexPattern.IsAutoName)
            {
                if (!_nextPatternAutoNames.Contains(lexPattern.Name))
                {
                    _nextPatternAutoNames.Insert(0, lexPattern.Name);
                }
            }

            ConsumeNextPatternName();
        }

        public void RefreshPattern(LexPatternViewModel lexPattern)
        {
            LexGraphViewModel.RefreshLexPatterns();
        }
    }

    public class LexPatternViewModel : ViewModel
    {
        private string _initialName;

        public LexPatternViewModel(string pattern, string name)
        {
            Pattern = pattern;
            Name = name;
        }

        public bool IsAutoName { get; set; }

        public string Name
        {
            get { return get(() => Name); }
            set
            {
                if (value != _initialName)
                    IsAutoName = false;
                else
                    IsAutoName = true;

                set(() => Name, value);
            }
        }

        public string Pattern
        {
            get { return get(() => Pattern); }
            set { set(()=>Pattern, value); }
        }
    }
}
