using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntlrTest
{
    public class RapidBnf_Antlr_Test : RapidBnf_Antlr_ParseTestBase
    {
        protected override void Benchmark()
        {
            Parse(Text_Rbnf_1000_Lines);
        }

        protected override void Test()
        {
            ParseRepeated(Text_Rbnf_1000_Lines, 100);
            ParseUntilUserInput(Text_Rbnf_1000_Lines);
        }
    }
}
