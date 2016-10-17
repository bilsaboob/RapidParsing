using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Util;

namespace RapidPliant.Lexing.Pattern.Regex
{
    public class RegexPatternExpr : PatternExpr<RegexPatternExpr>, IPatternExpr
    {
        public RegexPatternExpr()
            : this(false, false)
        {
        }

        protected RegexPatternExpr(bool isAlteration, bool isProduction)
            : base(isAlteration, isProduction)
        {
        }

        protected override RegexPatternExpr CreateAlterationExpr()
        {
            return new RegexPatternExpr(true, false);
        }

        protected override RegexPatternExpr CreateProductionExpr()
        {
            return new RegexPatternExpr(false, true);
        }

        protected override string _ToStringRef()
        {
            return ToString();
        }

        public static RegexPatternExpr Production()
        {
            return new RegexPatternExpr(false, true);
        }

        public static RegexPatternExpr Alteration()
        {
            return new RegexPatternExpr(true, false);
        }
    }

    public class RegexRootExpr : RegexPatternExpr
    {
        public RegexRootExpr()
            : base(false, true)
        {
        }
    }
    
    public class RegexTerminalExpr : RegexPatternExpr, IPatternTerminalCharExpr
    {
        public RegexTerminalExpr(char character)
        {
            Char = character;
        }

        public char Char { get; set; }

        protected override void _ToStringExpr(IText text)
        {
            text.Append(Char.ToString());
        }
    }

    public class RegexRangeExpr : RegexPatternExpr, IPatternTerminalRangeExpr
    {
        public RegexRangeExpr(char fromChar, char toChar)
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
