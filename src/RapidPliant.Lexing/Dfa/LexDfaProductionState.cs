using System.Text;
using RapidPliant.Common.Rule;
using RapidPliant.Common.Util;

namespace RapidPliant.Lexing.Dfa
{
    public class LexDfaProductionState
    {
        private int _hashCode;

        public LexDfaProductionState(LexDfaProductionPath forPath)
        {
            Path = forPath;

            Production = forPath.Production;
            SymbolIndex = forPath.SymbolIndex;
            Symbol = forPath.Symbol;
            IsAtEnd = forPath.IsAtEnd;
            
            _hashCode = HashCode.Compute(Production.LhsRule.GetHashCode(), SymbolIndex.GetHashCode());
        }

        public IProduction Production { get; private set; }
        public int SymbolIndex { get; private set; }
        public ISymbol Symbol { get; private set; }
        public bool IsAtEnd { get; private set; }

        public LexDfaProductionPath Path { get; private set; }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj == this)
                return true;

            var other = obj as LexDfaProductionState;
            if (other == null)
                return false;

            if (other.Production != Production)
                return false;

            if (other.Production == null)
                return false;

            if (Production == null)
                return false;

            if (!other.Production.Equals(Production))
                return false;

            return true;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            const string dotStr = "\u25CF";

            sb.AppendFormat("{0}->", Production.LhsRule.ToString());

            var rhsCount = Production.RhsSymbolsCount;
            var rhs = Production.RhsSymbols;
            for (var i = 0; i < rhsCount; ++i)
            {
                var symbol = rhs[i];

                if (i > 0)
                    sb.Append(" ");

                if (i == SymbolIndex && !(IsAtEnd && i == rhsCount - 1))
                {
                    sb.AppendFormat("{0}{1}", dotStr, symbol.ToString());
                }
                else
                {
                    sb.AppendFormat("{0}", symbol.ToString());
                }
            }

            if (IsAtEnd)
            {
                sb.Append(dotStr);
            }

            return sb.ToString();
        }
    }
}