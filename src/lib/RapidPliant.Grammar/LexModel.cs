using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Util;

namespace RapidPliant.Grammar
{
    public abstract class LexModel : ILexModel
    {
    }

    public class LexTerminalModel : LexModel, ILexTerminalModel
    {
        public LexTerminalModel(char character)
        {
            Char = character;
        }

        public char Char { get; private set; }

        public override string ToString()
        {
            return $"'{Char}'";
        }
    }

    public class LexSpellingModel : LexModel, ILexSpellingModel
    {
        public LexSpellingModel(string spelling)
        {
            Spelling = spelling;
        }

        public string Spelling { get; private set; }

        public override string ToString()
        {
            return $"\"{Spelling}\"";
        }
    }

    public class LexPatternModel : LexModel, ILexPatternModel
    {
        public LexPatternModel(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; private set; }

        public override string ToString()
        {
            return $"<{Pattern}>";
        }
    }
}
