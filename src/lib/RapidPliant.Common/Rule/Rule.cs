using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Util;

namespace RapidPliant.Common.Rule
{
    public class RuleSet<TRule>
    {
        public List<ISymbolRef> Symbols { get; set; }
        public List<ISymbolRef> LexSymbols { get; set; }
        public List<ISymbolRef> RuleSymbols { get; set; }

        public List<TRule> Rules { get; set; }
        public List<TRule> StartRules { get; set; }
    }

    public interface ISymbolRef
    {
        IProduction Production { get; }
        int Location { get; }
        ISymbol Symbol { get; }
    }

    public class SymbolRef : ISymbolRef
    {
        public SymbolRef(IProduction production, int location)
        {
            Production = production;
            Location = location;
            Symbol = production.RhsSymbols[location];
        }

        public IProduction Production { get; set; }
        public int Location { get; set; }
        public ISymbol Symbol { get; set; }
    }

    public interface IRule
    {
        string Name { get; }

        bool IsNullable { get; }

        bool IsRootRule { get; }
        bool IsSubRule { get; }
        IRule RootRule { get; }

        bool HasParentRule { get; }
        IRule ParentRule { get; }

        bool HasSubRules { get; }
        IRapidList<IRule> SubRules { get; }
        IRule CreateSubRule();
        void AddSubRule(IRule rule);

        IRapidList<IProduction> Productions { get; }
        IProduction CreateProduction();
        void AddProduction(IProduction production);
    }

    public class Rule<TRule> : IRule
        where TRule : Rule<TRule>, new()
    {
        protected TRule _this;

        private RapidList<TRule> _subRules;
        private RapidList<Production<TRule>> _productions;

        public Rule()
        {
            _this = (TRule)this;

            IsRootRule = true;
            IsSubRule = false;
            RootRule = _this;
            HasParentRule = false;
            ParentRule = default(TRule);

            _subRules = new RapidList<TRule>();
            _productions = new RapidList<Production<TRule>>();
        }

        private void SetParentRule(TRule parent)
        {
            ParentRule = parent;
            IsRootRule = false;
            IsSubRule = true;
            RootRule = parent.RootRule;
            HasParentRule = true;
            ParentRule = parent;
        }

        public string Name { get; set; }

        public bool IsNullable { get; set; }

        public bool IsRootRule { get; set; }
        public bool IsSubRule { get; set; }
        public TRule RootRule { get; set; }
        IRule IRule.RootRule { get { return RootRule; } }

        public bool HasParentRule { get; set; }
        public TRule ParentRule { get; private set; }
        IRule IRule.ParentRule { get { return ParentRule; } }

        public bool HasSubRules { get { return _subRules.Count > 0; } }
        IRapidList<IRule> IRule.SubRules { get { return _subRules; } }
        public IRapidList<TRule> SubRules { get { return _subRules; } }
        public virtual TRule CreateSubRule()
        {
            var subRule = new TRule();
            subRule.SetParentRule(_this);
            return subRule;
        }
        IRule IRule.CreateSubRule()
        {
            return CreateSubRule();
        }
        public void AddSubRule(TRule rule)
        {
            _subRules.Add(rule);
        }
        void IRule.AddSubRule(IRule rule)
        {
            var ruleInternal = rule as TRule;
            if(ruleInternal == null)
                return;
            AddSubRule(ruleInternal);
        }

        public IRapidList<Production<TRule>> Productions { get { return _productions; } }
        IRapidList<IProduction> IRule.Productions { get { return _productions; } }

        public virtual Production<TRule> CreateProduction()
        {
            return new Production<TRule>(_this);
        }
        IProduction IRule.CreateProduction()
        {
            return CreateProduction();
        }
        public void AddProduction(Production<TRule> production)
        {
            _productions.Add(production);
        }
        void IRule.AddProduction(IProduction production)
        {
            var productionInternal = production as Production<TRule>;
            if(productionInternal == null)
                return;
            AddProduction(productionInternal);
        }
    }

    public interface IProduction
    {
        IRule LhsRule { get; }
        int RhsSymbolsCount { get; }
        ISymbol[] RhsSymbols { get; }
    }

    public class Production<TRule> : IProduction
        where TRule : Rule<TRule>, new()
    {
        private CachingRapidList<ISymbol> _rhsSymbols;

        public Production(TRule lhsRule)
        {
            LhsRule = lhsRule;
            _rhsSymbols = new CachingRapidList<ISymbol>();
        }

        public TRule LhsRule { get; private set; }
        public int RhsSymbolsCount { get { return _rhsSymbols.Count; } }
        public IRapidList<ISymbol> RhsSymbols { get { return _rhsSymbols; } }

        IRule IProduction.LhsRule { get { return LhsRule; } }
        ISymbol[] IProduction.RhsSymbols { get { return _rhsSymbols.AsArray; } }
    }
}
