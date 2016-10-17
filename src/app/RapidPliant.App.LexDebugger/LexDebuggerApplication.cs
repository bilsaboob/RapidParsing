using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RapidPliant.App.LexDebugger.Views;

namespace RapidPliant.App.LexDebugger
{
    public class LexDebuggerApplication : RapidPliantAppliction
    {
        protected override Control CreateMainContent()
        {
            return new AppView();
        }
    }
}
