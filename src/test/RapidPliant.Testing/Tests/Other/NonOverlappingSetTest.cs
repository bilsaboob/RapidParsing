using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Test;
using RapidPliant.Util;

namespace RapidPliant.Testing.Tests
{
    public class NonOverlappingSetTest : TestBase
    {
        protected override void Test()
        {
            var set = new NonOverlappingIntervalSet<string>();

            var s = "";
            set.AddInterval(new Interval('a', 'e'), "1");            
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('i', 'k'), "2");
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('d', 'f'), "3");
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('b', 'c'), "4");
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('b', 'j'), "5");
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('n', 'o'), "6");
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('m', 'o'), "7");
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('m', 'p'), "8");
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('n', 'p'), "9");
            s = set.ToString();
            Console.WriteLine(s);
            set.AddInterval(new Interval('a', 'z'), "10");
            s = set.ToString();
            Console.WriteLine(s);

            Console.WriteLine();
             
            foreach (var node in set)
            {
                var items = string.Join(",", node.AssociatedItems);
                Console.WriteLine($"Associated items for '{node.Interval}': {items}" );
            }
        }
    }
}
