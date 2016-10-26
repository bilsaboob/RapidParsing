using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Test;
using RapidPliant.Testing.Tests;

namespace RapidPliant.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRunner.Run<RegexPatternTest>();
            //TestRunner.Run<NonOverlappingSetTest>();
        }
    }
}
