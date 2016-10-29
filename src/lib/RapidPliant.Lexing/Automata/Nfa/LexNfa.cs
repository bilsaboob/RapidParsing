using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Symbols;

namespace RapidPliant.Lexing.Automata
{
    public interface ILexNfaTransition : INfaTransition
    {
    }

    public interface ILexNfaNullTransition : ILexNfaTransition, INfaNullTransition
    {
    }

    public interface ILexNfaSymbolTransition : ILexNfaTransition
    {
    }

    public interface ILexNfaTerminalTransition : ILexNfaSymbolTransition
    {
        ITerminal Terminal { get; }
    }

    public class LexNfaTransition : NfaTransition, ILexNfaTransition
    {
    }

    public abstract class LexNfaSymbolTransition : LexNfaTransition, ILexNfaSymbolTransition
    {
        public LexNfaSymbolTransition(ISymbol symbol)
        {
            Symbol = symbol;
        }

        public ISymbol Symbol { get; protected set; }

        protected override string ToTransitionSymbolString()
        {
            if (Symbol == null)
                return "";

            return Symbol.ToString();
        }
    }

    public class LexNfaTerminalTransition : LexNfaSymbolTransition, ILexNfaTerminalTransition
    {
        public LexNfaTerminalTransition(ITerminal terminal)
            : base(terminal)
        {
            Terminal = terminal;
        }

        public ITerminal Terminal { get; protected set; }
    }

    public class LexNfaNullTransition : LexNfaTransition, ILexNfaNullTransition
    {
        protected override string ToTransitionArrowString()
        {
            return "=>";
        }
    }

}
