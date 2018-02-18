using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Test
{
    public static class TestRunner
    {
        public static void Run<TTest>()
            where TTest : ITest, new()
        {
            var test = new TTest();
            test.Setup();
            test.Run();

            Console.Read();
        }
    }
}
