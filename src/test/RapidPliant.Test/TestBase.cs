using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Test
{
    public abstract class TestBase : ITest
    {
        public bool Run()
        {
            Test();
            return true;
        }

        protected virtual void Test()
        {
            //Do the parsing
        }
    }
}
