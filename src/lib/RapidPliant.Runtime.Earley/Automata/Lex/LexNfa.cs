using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Runtime.Earley.Automata.Lex
{
    public interface ILexNfa
    {
        int LocalIndex { get; set; }
    }

    public interface ILexNfaState
    {
    }

    public interface ILexNfaTransition
    {
    }
}
