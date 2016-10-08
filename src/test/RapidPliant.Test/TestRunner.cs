using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Test
{
    public static class TestRunner
    {
        public static void Run<TTest>(bool debug = false)
            where TTest : ITest, new()
        {
            var test = new TTest();
            test.Run();

            Console.Read();
        }
    }
}
