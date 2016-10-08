using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Runtime.Util;

namespace RapidPliant.Runtime.Earley.Grammar
{
    public interface ISymbol
    {
    }

    public interface IProductionLhs
    {
        object Id { get; }
        string Name { get; }
    }

    public interface IProduction
    {
        IProductionLhs Lhs { get; }

        IProductionRhs Rhs { get; }

        bool IsEmpty { get; }
    }

    public interface IProductionRhs : IEnumerable<ISymbol>
    {
        bool IsEmpty { get; }

        int Count { get; }

        ISymbol this[int index] { get; }

        void Add(ISymbol symbol);

        void AddAll(IProduction fromProduction);
    }

    public class Production : IProduction
    {
        private ProductionRhs _rhs;

        public Production(IProductionLhs lhs)
            : this(lhs, new List<ISymbol>())
        {
        }

        public Production(IProductionLhs lhs, IEnumerable<ISymbol> rhsSymbols)
            : this(lhs, rhsSymbols.ToList())
        {
        }

        public Production(IProductionLhs lhs, List<ISymbol> rhsSymbols)
        {
            Lhs = lhs;
            _rhs = new ProductionRhs(rhsSymbols);
        }

        public IProductionLhs Lhs { get; private set; }

        public IProductionRhs Rhs { get { return _rhs; } }

        public bool IsEmpty { get { return _rhs.IsEmpty; } }

        public Production Clone()
        {
            return new Production(Lhs, _rhs._symbols);
        }
    }

    public class ProductionRhs : IProductionRhs
    {
        internal List<ISymbol> _symbols;
        private ISymbol[] _symbolsCached;

        private ISymbol[] SymbolsCached
        {
            get
            {
                if (_symbolsCached == null || _symbolsCached.Length != _symbols.Count)
                {
                    _symbolsCached = _symbols.ToArray();
                }
                return _symbolsCached;
            }
        }

        public ProductionRhs()
            : this(new List<ISymbol>())
        {
        }

        public ProductionRhs(List<ISymbol> symbols)
        {
            _symbols = symbols;
        }

        public bool IsEmpty { get { return _symbols.Count == 0; } }

        public int Count { get { return _symbols.Count; } }

        public void AddAll(IProduction fromProduction)
        {
            if(fromProduction.IsEmpty)
                return;

            foreach (var symbol in fromProduction.Rhs)
            {
                Add(symbol);
            }
        }

        public void Add(ISymbol symbol)
        {
            _symbols.Add(symbol);
        }

        public ISymbol this[int index]
        {
            get { return SymbolsCached[index]; }
        }

        public IEnumerator<ISymbol> GetEnumerator()
        {
            return new ArrayEnumerator<ISymbol>(SymbolsCached);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
