using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Util;

namespace RapidPliant.Common.Symbols
{
    public interface ITerminal : ISymbol
    {
        bool IsMatch(char character);

        IReadOnlyList<Interval> GetIntervals();
    }

    public abstract class BaseTerminal : Symbol, ITerminal
    {
        protected BaseTerminal()
            : base(SymbolType.Terminal)
        {
        }

        public abstract bool IsMatch(char character);

        public abstract IReadOnlyList<Interval> GetIntervals();
    }
}
