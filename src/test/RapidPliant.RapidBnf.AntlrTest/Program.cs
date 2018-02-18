using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.RapidBnf.Test.Tests.Grammar;
using RapidPliant.Test;

namespace AntlrTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRunner.Run<RapidBnf_Antlr_Test>();

            //TestRunner.Run<RapidSharp_Antlr_Test>();
        }
    }
}
