using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

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
            ParseRepeated(Text_Rbnf_1000_Lines, 1);
            ParseUntilUserInput(Text_Rbnf_1000_Lines);
        }

        protected override Lexer CreateLexer(AntlrInputStream inputStream)
        {
            return new RBNFLexer(inputStream);
        }

        protected override Parser CreateParser(CommonTokenStream tokenStream)
        {
            return new RBNFParser(tokenStream);
        }

        protected override object Parse(Parser parser)
        {
            return ((RBNFParser)parser).document();
        }
    }
}
