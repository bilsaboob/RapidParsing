using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Lexing.Pattern.Regex
{
    public class RapidRegex
    {
        private RegexPatternParser _regexPatternParser;

        public RapidRegex()
        {
            _regexPatternParser = new RegexPatternParser();
        }

        [DebuggerStepThrough]
        public RegexExpr FromPattern(string pattern)
        {
            return _regexPatternParser.Parse(new StringReader(pattern));
        }
    }
}
