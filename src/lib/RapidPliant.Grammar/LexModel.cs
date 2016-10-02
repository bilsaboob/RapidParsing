using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /*public override string ToStringRef()
        {
            if (string.IsNullOrEmpty(Name))
                return ToString();

            return base.ToStringRef();
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("'" + Char.ToString() + "'");
        }*/
    }

    public class LexSpellingModel : LexModel, ILexSpellingModel
    {
        public LexSpellingModel(string spelling)
        {
            Spelling = spelling;
        }

        public string Spelling { get; private set; }

        /*public override string ToStringRef()
        {
            if (string.IsNullOrEmpty(Name))
                return ToString();

            return base.ToStringRef();
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("\"" + Spelling + "\"");
        }*/
    }

    public class LexPatternModel : LexModel, ILexPatternModel
    {
        public LexPatternModel(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; private set; }

        /*public override string ToStringRef()
        {
            if (string.IsNullOrEmpty(Name))
                return ToString();

            return base.ToStringRef();
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("<" + Pattern + ">");
        }*/
    }
}
