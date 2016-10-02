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

        internal virtual ILexModel CreateNumberExpr(string name = null)
        {
            return CreateLexPatternExpr("[0-9]+");
        }
        
        internal virtual ILexModel CreateStringQuotedExpr(string name = null)
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

        protected ILexModel Number(string name = null)
        {
            return CreateNumberExpr(name);
        }

        protected ILexModel StringQuoted(string name = null)
        {
            return CreateStringQuotedExpr(name);
        }

        #endregion
    }
}