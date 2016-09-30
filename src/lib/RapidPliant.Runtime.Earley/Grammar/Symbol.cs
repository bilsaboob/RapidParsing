using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Runtime.Earley.Grammar
{
    public interface ISymbol
    {
    }

    public interface ILexSymbol : ISymbol
    {
    }

    public interface ITerminalSymbol : ILexSymbol
    {
    }
}
