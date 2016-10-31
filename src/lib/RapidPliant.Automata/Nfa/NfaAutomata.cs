using System.Collections.Generic;
using System.Linq;
using RapidPliant.Common.Expression;

namespace RapidPliant.Automata.Nfa
{
    public abstract class NfaAutomata<TAutomata>
        where TAutomata : NfaAutomata<TAutomata>, new()
    {
        #region static factory helpers
        private static TAutomata _instance;
        private static TAutomata Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TAutomata();
                }

                return _instance;
            }
        }

        public static INfa BuildNfa(IExpr expr)
        {
            return Instance.CreateNfaForExpression(expr);
        }

        public static INfa BuildNfa(IEnumerable<IExpr> expressions)
        {
            return Instance.CreateNfaForExpressions(expressions);
        }

        #endregion
        
        public virtual INfa CreateNfaForExpression(IExpr expr)
        {
            return CreateBuilder().Create(expr);
        }

        protected abstract INfaBuilder CreateBuilder();

        public INfa CreateNfaForExpressions(params IExpr[] expressions)
        {
            if (expressions == null)
                return null;

            return CreateNfaForExpressions(expressions.AsEnumerable());
        }

        public INfa CreateNfaForExpressions(IEnumerable<IExpr> expressions)
        {
            if (expressions == null)
                return null;

            return CreateBuilder().Create(expressions);
        }
    }
}
