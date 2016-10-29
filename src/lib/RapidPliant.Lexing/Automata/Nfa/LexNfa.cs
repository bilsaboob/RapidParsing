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

    public partial class LexNfaAutomata
    {
        public class LexNfa : NfaBase
        {
            public LexNfa(LexNfaState start, LexNfaState end) : base(start, end) { }

            public override INfaGraph ToNfaGraph()
            {
                return LexAutomataExtensions.ToNfaGraph(this);
            }
        }

        public class LexNfaState : NfaStateBase
        {
        }

        public class LexNfaTransition : NfaTransitionBase, ILexNfaTransition
        {
            public LexNfaTransition(LexNfaState toState) : base(null, toState)
            {
            }
        }

        public abstract class SymbolNfaTransition : LexNfaTransition, ILexNfaSymbolTransition
        {
            public SymbolNfaTransition(ISymbol symbol, LexNfaState toState)
                : base(toState)
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

        public class TerminalNfaTransition : SymbolNfaTransition, ILexNfaTerminalTransition
        {
            public TerminalNfaTransition(ITerminal terminal, LexNfaState toState)
                : base(terminal, toState)
            {
                Terminal = terminal;
            }

            public ITerminal Terminal { get; protected set; }
        }

        public class NullNfaTransition : LexNfaTransition, ILexNfaNullTransition
        {
            public NullNfaTransition(LexNfaState toState)
                : base(toState)
            {
            }

            protected override string ToTransitionArrowString()
            {
                return "=>";
            }
        }
    }

}
