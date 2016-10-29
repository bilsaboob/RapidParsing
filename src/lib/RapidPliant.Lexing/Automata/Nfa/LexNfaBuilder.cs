using System;
using RapidPliant.Automata.Nfa;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Symbols;
using RapidPliant.Lexing.Pattern;

namespace RapidPliant.Lexing.Automata.Nfa
{
    public class LexNfaBuilder : NfaBuilder
    {
        public LexNfaBuilder()
        {
        }

        protected override INfa BuildForLeafExpression(IExpr expr)
        {
            var termExpr = expr as IPatternTerminalCharExpr;
            if (termExpr != null)
            {
                return NfaForChar(termExpr.Char, expr);
            }

            var termRangeExpr = expr as IPatternTerminalCharRangeExpr;
            if (termRangeExpr != null)
            {
                return NfaForCharRange(termRangeExpr.FromChar, termRangeExpr.ToChar, expr);
            }

            throw new Exception($"Unhandled leaf expression '{expr.ToString()}' of type '{expr.GetType().Name}'!");
        }

        #region Factory overrides

        protected override INfaTransition CreateNullTransition(INfaState fromState, INfaState toState, IExpr forExpression)
        {
            var t = new LexNfaNullTransition();
            t.EnsureToState(toState);
            return t;
        }

        #endregion

        #region Nfa factories
        protected virtual INfa CreateTerminalNfa(INfaState start, INfaState end, ITerminal terminal, IExpr expr)
        {
            return new RapidPliant.Automata.Nfa.Nfa(start, end);
        }

        protected virtual LexNfaTransition CreateTerminalTransition(INfaState fromState, INfaState toState, ITerminal terminal, IExpr expr)
        {
            var t = new LexNfaTerminalTransition(terminal);
            t.EnsureToState(toState);
            return t;
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

        public virtual INfa NfaForAnyChar(IExpr expr)
        {
            return NfaForTerminal(CreateAnyTerminal(), expr);
        }

        public virtual INfa NfaForChar(char character, IExpr expr)
        {
            return NfaForTerminal(CreateCharTerminal(character), expr);
        }

        public virtual INfa NfaForCharRange(char startChar, char endChar, IExpr expr, bool negate = false)
        {
            BaseTerminal terminal = new CharacterRangeTerminal(startChar, endChar);
            if (negate)
                terminal = new NegationTerminal(terminal);

            return NfaForTerminal(terminal, expr);
        }

        public virtual INfa NfaForCharClass(string charClass, IExpr expr, bool negate = false)
        {
            return NfaForTerminal(CreateCharClassTerminal(charClass, negate), expr);
        }

        protected virtual INfa NfaForTerminal(ITerminal terminal, IExpr expr)
        {
            var start = CreateNfaState();
            var end = CreateNfaState();

            var termTransition = CreateTerminalTransition(start, end, terminal, expr);
            AssociateTransitionWithExpression(termTransition, expr);
            start.AddTransition(termTransition);

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
}
