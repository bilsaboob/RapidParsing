using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Lexing.Pattern;

namespace RapidPliant.Lexing.Automata
{
    public partial class LexNfaAutomata
    {
        public class LexNfaBuildContext : NfaBuildContextBase
        {
            public LexNfaBuildContext()
            {
            }

            #region Factory overrides
            protected override LexNfa CreateNfa(LexNfaState start, LexNfaState end, IExpr forExpression)
            {
                return new LexNfa(start, end);
            }

            protected override LexNfa CreateEmptyNfa(LexNfaState start, LexNfaState end, IExpr forExpression)
            {
                return new LexNfa(start, end);
            }

            protected override LexNfa CreateOrNfa(LexNfaState start, LexNfaState end, IExpr orExpr, IEnumerable<IExpr> forExpressions)
            {
                return new LexNfa(start, end);
            }

            protected override LexNfa CreateAndNfa(LexNfaState start, LexNfaState end, IExpr forExpression)
            {
                return new LexNfa(start, end);
            }

            protected override LexNfa CreateRepeatNfa(LexNfaState start, LexNfaState end, IExpr forExpression)
            {
                return new LexNfa(start, end);
            }

            protected override LexNfa CreateOptionalNfa(LexNfaState start, LexNfaState end, IExpr forExpression)
            {
                return new LexNfa(start, end);
            }

            protected override LexNfaState CreateNfaState()
            {
                return new LexNfaState();
            }

            protected override LexNfaTransition CreateOpenOrTransition(LexNfaState fromState, LexNfaState toState, IExpr orExpr, IExpr forExpression)
            {
                return new NullNfaTransition(toState);
            }

            protected override LexNfaTransition CreateCloseOrTransition(LexNfaState fromState, LexNfaState toState, IExpr orExpr, IExpr forExpression)
            {
                return new NullNfaTransition(toState);
            }

            protected override LexNfaTransition CreateAndJoinTransition(LexNfaState fromState, LexNfaState toState, IExpr andExpr, IExpr forExpression)
            {
                return new NullNfaTransition(toState);
            }

            protected override LexNfaTransition CreateRepeatStartTransition(LexNfaState fromState, LexNfaState toState, IExpr forExpression)
            {
                return new NullNfaTransition(toState);
            }

            protected override LexNfaTransition CreateRepeatEndTransition(LexNfaState fromState, LexNfaState toState, IExpr forExpression)
            {
                return new NullNfaTransition(toState);
            }

            protected override LexNfaTransition CreateOptionalStartTransition(LexNfaState fromState, LexNfaState toState, IExpr forExpression)
            {
                return new NullNfaTransition(toState);
            }

            protected override LexNfaTransition CreateOptionalEndTransition(LexNfaState fromState, LexNfaState toState, IExpr forExpression)
            {
                return new NullNfaTransition(toState);
            }

            protected override LexNfaTransition CreateEmptyStartToEndTransition(LexNfaState fromState, LexNfaState toState, IExpr forExpression)
            {
                return new NullNfaTransition(toState);
            }

            #endregion

            #region Nfa factories
            protected virtual LexNfa CreateTerminalNfa(LexNfaState start, LexNfaState end, ITerminal terminal, IExpr expr)
            {
                return new LexNfa(start, end);
            }

            protected virtual LexNfaTransition CreateTerminalTransition(LexNfaState fromState, LexNfaState toState, ITerminal terminal, IExpr expr)
            {
                return new TerminalNfaTransition(terminal, toState);
            }
            #endregion

            #region Terminal factory helpers
            protected virtual ITerminal CreateAnyTerminal()
            {
                return new AnyTerminal();
            }
            protected virtual ITerminal CreateCharTerminal(char character)
            {
                return new CharacterTerminal(character);
            }
            #endregion

            #region Factory helpers

            public virtual LexNfa NfaForAnyChar(IExpr expr)
            {
                return NfaForTerminal(CreateAnyTerminal(), expr);
            }

            public virtual LexNfa NfaForChar(char character, IExpr expr)
            {
                return NfaForTerminal(CreateCharTerminal(character), expr);
            }

            public virtual LexNfa NfaForCharRange(char startChar, char endChar, IExpr expr, bool negate = false)
            {
                BaseTerminal terminal = new CharacterRangeTerminal(startChar, endChar);
                if (negate)
                    terminal = new NegationTerminal(terminal);

                return NfaForTerminal(terminal, expr);
            }

            public virtual LexNfa NfaForCharClass(string charClass, IExpr expr, bool negate = false)
            {
                return NfaForTerminal(CreateCharClassTerminal(charClass, negate), expr);
            }

            protected virtual LexNfa NfaForTerminal(ITerminal terminal, IExpr expr)
            {
                var start = CreateNfaState();
                var end = CreateNfaState();

                var termTransition = CreateTerminalTransition(start, end, terminal, expr);
                AssociateTransitionWithExpression(termTransition, expr);
                start.AddTransistion(termTransition);

                var terminalNfa = CreateTerminalNfa(start, end, terminal, expr);
                AssociateNfaWithExpression(terminalNfa, expr);

                return terminalNfa;
            }

            protected virtual BaseTerminal CreateCharClassTerminal(string charClass, bool negate)
            {
                BaseTerminal terminal = null;
                switch (charClass)
                {
                    case "s":
                    terminal = new WhitespaceTerminal();
                    break;
                    case "d":
                    terminal = new DigitTerminal();
                    break;
                    case "w":
                    terminal = new WordTerminal();
                    break;
                    case "D":
                    terminal = new DigitTerminal();
                    negate = false;
                    break;
                    case "S":
                    terminal = new WhitespaceTerminal();
                    negate = false;
                    break;
                    case "W":
                    terminal = new WordTerminal();
                    negate = false;
                    break;
                }

                if (terminal == null)
                    throw new Exception($"Unhandled character class '{charClass}'!");

                if (negate)
                    terminal = new NegationTerminal(terminal);

                return terminal;
            }
            #endregion

        }

        public class LexNfaBuilder : NfaBuilderBase
        {
            public LexNfaBuilder()
            {
            }

            protected override LexNfa BuildForLeafExpression(LexNfaBuildContext c, IExpr expr)
            {
                var termExpr = expr as IPatternTerminalCharExpr;
                if (termExpr != null)
                {
                    return c.NfaForChar(termExpr.Char, expr);
                }

                var termRangeExpr = expr as IPatternTerminalCharRangeExpr;
                if (termRangeExpr != null)
                {
                    return c.NfaForCharRange(termRangeExpr.FromChar, termRangeExpr.ToChar, expr);
                }

                throw new Exception($"Unhandled leaf expression '{expr.ToString()}' of type '{expr.GetType().Name}'!");
            }

        }
    }
}
