using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RapidPliant.App.GrammarDebugger.Views;

namespace RapidPliant.App.GrammarDebugger
{
    public class GrammarDebuggerApplication : RapidPliantAppliction
    {
        protected override Control CreateMainContent()
        {
            return new AppView();
        }
    }
}
