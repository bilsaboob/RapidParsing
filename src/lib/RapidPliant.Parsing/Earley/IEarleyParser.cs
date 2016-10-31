using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Parsing.Earley
{
    public interface IEarleyParser
    {
        void Parse(TextReader input);
    }

    public interface IEarleyEngine
    {
        bool Parse(IParseContext parseContext);
    }

    public interface IParseContext
    {
        Token TokenToParse { get; set; }
    }

    public class ParseContext : IParseContext
    {
        public Token TokenToParse { get; set; }
    }
}
