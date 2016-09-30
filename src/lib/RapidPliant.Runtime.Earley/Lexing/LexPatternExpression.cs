using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Runtime.Earley.Lexing
{
    public class LexPatternExpression
    {
        private LexPatternParser PatternParser { get; set; }

        public LexPatternExpression(string pattern)
        {
            Pattern = pattern;
            PatternParser = new LexPatternParser();
        }

        public string Pattern { get; private set; }

        public ILexExpression LexExpression { get; private set; }

        public void Compile()
        {
            LexExpression = PatternParser.Parse(Pattern);
        }
    }
}
