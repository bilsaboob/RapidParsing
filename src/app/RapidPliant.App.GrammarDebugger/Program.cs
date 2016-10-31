using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.App.GrammarDebugger
{
    public static class Program
    {
        [STAThreadAttribute()]
        public static void Main()
        {
            //Run the lex debugger application
            AppBoostrapper.Run<GrammarDebuggerApplication>();
        }
    }
}
