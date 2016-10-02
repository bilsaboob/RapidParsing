using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public abstract class SimpleGrammarModel<TGrammarModel> : GrammarModel<TGrammarModel>
        where TGrammarModel : SimpleGrammarModel<TGrammarModel>
    {
        private NullExpr _nullExpr = new NullExpr();
        
        #region Customizable helpers

        protected virtual NullExpr CreateNullExpr()
        {
            return new NullExpr();
        }

        internal virtual Lex CreateNumberExpr(string name = null)
        {
            return CreateLexPatternExpr("[0-9]+");
        }
        
        internal virtual Lex CreateStringQuotedExpr(string name = null)
        {
            return CreateLexPatternExpr("[\"][^\"]+[\"]");
        }
        
        #endregion

        #region Helpers

        protected NullExpr Null
        {
            get
            {
                if (_nullExpr != null)
                {
                    _nullExpr = CreateNullExpr();
                }
                return _nullExpr;
            }
        }

        protected Lex Number(string name = null)
        {
            return CreateNumberExpr(name);
        }

        protected Lex StringQuoted(string name = null)
        {
            return CreateStringQuotedExpr(name);
        }

        #endregion
    }
}