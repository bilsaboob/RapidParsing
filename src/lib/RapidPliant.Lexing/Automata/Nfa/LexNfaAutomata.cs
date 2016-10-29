using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;

namespace RapidPliant.Lexing.Automata
{
    //Specification for Lex automata
    public class LexNfaAutomataSpecification : NfaAutomataSpecification<
            LexNfaAutomata,
            //Nfa graph specification
            LexNfaAutomata.LexNfaGraph,
            LexNfaAutomata.LexNfaGraphBuildContext,
            //Nfa specification
            LexNfaAutomata.LexNfa,
            LexNfaAutomata.LexNfaState,
            LexNfaAutomata.LexNfaTransition,
            //Nfa builder specification
            LexNfaAutomata.LexNfaBuilder,
            LexNfaAutomata.LexNfaBuildContext
        >
    {
    }

    //Helper for partial separation
    public partial class LexNfaAutomata : LexNfaAutomataSpecification
    {
        #region static factory helpers
        public static LexNfa BuildNfa(IExpr expr)
        {
            return Instance.CreateNfaForExpression(expr);
        }

        public static LexNfa BuildNfa(IEnumerable<IExpr> expressions)
        {
            return Instance.CreateNfaForExpressions(expressions);
        }
        #endregion

        public LexNfaBuilder NfaBuilder { get; set; }

        public LexNfaAutomata()
        {
            NfaBuilder = new LexNfaBuilder();
        }

        public LexNfa CreateNfaForExpression(IExpr expr)
        {
            return NfaBuilder.Create(expr);
        }

        public LexNfa CreateNfaForExpressions(params IExpr[] expressions)
        {
            if (expressions == null)
                return null;

            return CreateNfaForExpressions(expressions.AsEnumerable());
        }

        public LexNfa CreateNfaForExpressions(IEnumerable<IExpr> expressions)
        {
            if (expressions == null)
                return null;

            return NfaBuilder.Create(expressions);
        }
    }

    public static class LexAutomataExtensions
    {
        public static LexNfaAutomata.LexNfaGraph ToNfaGraph(this LexNfaAutomata.LexNfa nfa)
        {
            var g = new LexNfaAutomata.LexNfaGraph(nfa);
            return g;
        }
    }
}
