using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar.Definitions
{
    public class LexDef : GrammarDef, ILexDef
    {
        public LexDef()
        {
        }

        public ILexModel LexModel { get; protected set; }
    }

    public partial class Lex : LexDef
    {
        public void As(ILexModel lexModel)
        {
            LexModel = lexModel;
        }
    }

    public partial class Lex : LexDef
    {
        public static implicit operator GrammarExpr(Lex lex)
        {
            return GrammarDef.LexRef(lex);
        }

        public static implicit operator Lex(string lexName)
        {
            return new Lex() {
                Name = lexName
            };
        }

        #region And
        public static GrammarExpr operator +(Lex lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.LexRef(lhs), rhs);
        }
        public static GrammarExpr operator +(GrammarExpr lhs, Lex rhs)
        {
            return GrammarExpr.AddWithAnd(lhs, GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator +(Lex lhs, Lex rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.LexRef(lhs), GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator +(Lex lhs, char rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.LexRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator +(char lhs, Lex rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator +(Lex lhs, string rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.LexRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }

        public static GrammarExpr operator +(string lhs, Lex rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), GrammarDef.LexRef(rhs));
        }
        #endregion

        #region Or
        public static GrammarExpr operator |(GrammarExpr lhs, Lex rhs)
        {
            return GrammarExpr.AddWithOr(lhs, GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator |(Lex lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.LexRef(lhs), rhs);
        }
        public static GrammarExpr operator |(Lex lhs, Lex rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.LexRef(lhs), GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator |(Lex lhs, char rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.LexRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(char lhs, Lex rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator |(Lex lhs, string rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.LexRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(string lhs, Lex rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), GrammarDef.LexRef(rhs));
        }
        #endregion
    }

    public class InPlaceLexDef : Lex
    {
        public InPlaceLexDef(ILexModel lexModel)
        {
            LexModel = lexModel;
        }
    }

}
