using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public abstract class LexExpr : Expr, ILexExpr
    {
        public LexExpr()
        {
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public override void ToString(StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                sb.Append(Name);
            }
        }
    }

    public class Lex : LexExpr
    {
        public static implicit operator Lex(string name)
        {
            return new DeclarationLexExpr(name);
        }
    }

    public class DeclarationLexExpr : Lex, IExprDeclaration
    {
        public DeclarationLexExpr(string name)
        {
            Name = name;
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append(Name);
        }
    }

    public class LexTerminalExpr : Lex, ILexTerminalExpr
    {
        public LexTerminalExpr(char character)
        {
            Char = character;
        }

        public char Char { get; private set; }

        public override string ToStringRef()
        {
            if (string.IsNullOrEmpty(Name))
                return ToString();

            return base.ToStringRef();
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("'" + Char.ToString() + "'");
        }
    }

    public class LexSpellingExpr : Lex, ILexSpellingExpr
    {
        public LexSpellingExpr(string spelling)
        {
            Spelling = spelling;
        }

        public string Spelling { get; private set; }

        public override string ToStringRef()
        {
            if (string.IsNullOrEmpty(Name))
                return ToString();

            return base.ToStringRef();
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("\"" + Spelling + "\"");
        }
    }

    public class LexPatternExpr : Lex, ILexPatternExpr
    {
        public LexPatternExpr(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; private set; }

        public override string ToStringRef()
        {
            if (string.IsNullOrEmpty(Name))
                return ToString();

            return base.ToStringRef();
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("<" + Pattern + ">");
        }
    }
}
