using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar.Definitions
{
    public partial class LexDef : GrammarDef, ILexDef
    {
        public LexDef()
        {
        }

        public ILexModel LexModel { get; protected set; }

        public LexDef As(ILexModel lexModel)
        {
            LexModel = lexModel;
            return this;
        }

        public override string ToString()
        {
            return $"L:{Name}";
        }
    }
    

    public partial class LexDef
    {
        public static implicit operator GrammarExpr(LexDef lex)
        {
            return GrammarDef.LexRef(lex);
        }

        public static implicit operator LexDef(string lexName)
        {
            return new LexDef() {
                Name = lexName
            };
        }

        #region And
        /*public static GrammarExpr operator +(LexDef lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.LexRef(lhs), rhs);
        }*/
        /*public static GrammarExpr operator +(GrammarExpr lhs, LexDef rhs)
        {
            return GrammarExpr.AddWithAnd(lhs, GrammarDef.LexRef(rhs));
        }*/
        public static GrammarExpr operator +(LexDef lhs, LexDef rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.LexRef(lhs), GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator +(LexDef lhs, char rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.LexRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator +(char lhs, LexDef rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator +(LexDef lhs, string rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.LexRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }

        public static GrammarExpr operator +(string lhs, LexDef rhs)
        {
            return GrammarExpr.AddWithAnd(GrammarDef.InPlaceLexRef(lhs), GrammarDef.LexRef(rhs));
        }
        #endregion

        #region Or
        public static GrammarExpr operator |(LexDef lhs, GrammarExpr rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.LexRef(lhs), rhs);
        }
        public static GrammarExpr operator |(GrammarExpr lhs, LexDef rhs)
        {
            return GrammarExpr.AddWithOr(lhs, GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator |(LexDef lhs, LexDef rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.LexRef(lhs), GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator |(LexDef lhs, char rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.LexRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(char lhs, LexDef rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), GrammarDef.LexRef(rhs));
        }
        public static GrammarExpr operator |(LexDef lhs, string rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.LexRef(lhs), GrammarDef.InPlaceLexRef(rhs));
        }
        public static GrammarExpr operator |(string lhs, LexDef rhs)
        {
            return GrammarExpr.AddWithOr(GrammarDef.InPlaceLexRef(lhs), GrammarDef.LexRef(rhs));
        }
        #endregion
    }

    public class InPlaceLexDef : LexDef
    {
        public InPlaceLexDef(ILexModel lexModel)
        {
            LexModel = lexModel;
        }
    }

}
