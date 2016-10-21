using System.Text;
using RapidPliant.Common.Rule;

namespace RapidPliant.Lexing.Dfa
{
    public class LexDfaProductionPath
    {
        private IProduction _production;
        private int _rhsSymbolsCount;
        private ISymbol[] _rhsSymbols;
        private int _rhsIndex;
        private bool _isAtEnd;
        private int _rhsEndIndex;
        private ISymbol _symbol;
        private LexDfaState _state;
        private LexDfaProductionState _productionState;
        private LexDfaProductionPath _parentPath;

        public LexDfaProductionPath(IProduction production, LexDfaState state, LexDfaProductionPath parentPath = null)
        {
            _state = state;

            _production = production;
            _rhsIndex = 0;

            _rhsSymbolsCount = Production.RhsSymbolsCount;
            _rhsSymbols = Production.RhsSymbols;
            _rhsEndIndex = _rhsSymbolsCount - 1;

            _isAtEnd = _rhsIndex >= _rhsSymbolsCount;
            _symbol = _rhsSymbols[_rhsIndex];

            _parentPath = parentPath;
        }

        public LexDfaState State { get { return _state; } set { _state = value; } }

        public int SymbolIndex { get { return _rhsIndex; } }
        public IProduction Production { get { return _production; } }

        public bool IsAtEnd { get { return _isAtEnd; } }
        public ISymbol Symbol { get { return _symbol; } }

        public LexDfaProductionPath ParentPath { get { return _parentPath; } }
        public LexDfaStateTransition Transition { get; set; }

        public LexDfaProductionPath MoveToFirst()
        {
            MoveNext();
            return this;
        }

        public bool MoveNext()
        {
            if (_isAtEnd)
                return false;

            if (_rhsIndex >= _rhsEndIndex)
            {
                _isAtEnd = true;
                return false;
            }

            _rhsIndex++;
            _symbol = _rhsSymbols[_rhsIndex];
            _productionState = null;
            return true;
        }

        public LexDfaProductionState ToProductionState()
        {
            if (_productionState == null)
            {
                _productionState = new LexDfaProductionState(this);
            }

            return _productionState;
        }

        public LexDfaProductionPath Clone()
        {
            LexDfaProductionPath parentPath = null;
            if (_parentPath != null)
            {
                parentPath = _parentPath.Clone();
            }
            var path = new LexDfaProductionPath(_production, _state, parentPath);
            path._rhsIndex = _rhsIndex;
            path._rhsEndIndex = _rhsEndIndex;
            path._isAtEnd = _isAtEnd;
            return path;
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