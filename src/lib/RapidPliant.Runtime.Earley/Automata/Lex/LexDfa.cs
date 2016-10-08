using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Runtime.Earley.Automata.Earley;
using RapidPliant.Runtime.Earley.Parsing;

namespace RapidPliant.Runtime.Earley.Automata.Lex
{
    public interface ILexDfa
    {
        ILexDfaState StartState { get; }
    }

    public interface ILexDfaState
    {
        ILexDfaTransitionsByChar Transitions { get; set; }

        bool HasCompletions { get; set; }
        ILexCompletion[] Completions { get; set; }
    }

    public interface ILexDfaTransition
    {
        ILexDfaState ToState { get; }
    }

    public interface ILexDfaTransitionsByChar
    {
        ILexDfaTransition Get(char c);
    }

    public interface ILexCompletion
    {
    }
}
