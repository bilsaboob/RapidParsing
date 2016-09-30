using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Runtime.Earley.Lexing
{
    public class LexPatternParser
    {
        public LexPatternParser()
        {
        }

        public ILexExpression Parse(string pattern)
        {
            return Parse(new StringReader(pattern));
        }

        public ILexExpression Parse(TextReader input)
        {
            return null;
        }
    }
}
