﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.App.LexDebugger
{
    public static class Program
    {
        [STAThreadAttribute()]
        public static void Main()
        {
            //Run the lex debugger application
            AppBoostrapper.Run<LexDebuggerApplication>();
        }
    }
}
