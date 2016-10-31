using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Util;

namespace RapidPliant.Lexing.Pattern.Regex
{
    public class RegexExpr : PatternExpr<RegexExpr>, IPatternExpr
    {
        public RegexExpr()
            : this(false, false)
        {
        }

        protected RegexExpr(bool isAlteration, bool isProduction)
            : base(isAlteration, isProduction)
        {
        }

        protected override RegexExpr CreateAlterationExpr()
        {
            return new RegexExpr(true, false);
        }

        protected override RegexExpr CreateProductionExpr()
        {
            return new RegexExpr(false, true);
        }
        
        public static RegexExpr Production()
        {
            return new RegexExpr(false, true);
        }

        public static RegexExpr Alteration()
        {
            return new RegexExpr(true, false);
        }

        protected override string _ToStringRef()
        {
            return ToString();
        }
    }
    
    public class RegexCharExpr : RegexExpr, IPatternTerminalCharExpr
    {
        public RegexCharExpr(char character)
        {
            Char = character;
        }

        public char Char { get; set; }

        protected override void _ToStringExpr(IText text)
        {
            text.Append(Char.ToString());
        }
    }

    public class RegexCharRangeExpr : RegexExpr, IPatternTerminalCharRangeExpr
    {
        public RegexCharRangeExpr(char fromChar, char toChar)
        {
            FromChar = fromChar;
            ToChar = toChar;
        }

        public char FromChar { get; set; }
        public char ToChar { get; set; }

        protected override void _ToStringExpr(IText text)
        {
            text.Append(string.Format("{0}-{1}", FromChar, ToChar));
        }
    }
}
