using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Rule;
using RapidPliant.Common.Util;

namespace RapidPliant.Lexing.Pattern
{
    public class PatternRule<TRule> : Rule<TRule>
        where TRule : PatternRule<TRule>, new()
    {
        public PatternRule()
            : this(null)
        {
        }

        public PatternRule(string name)
            : base(name)
        {
        }

        public void FromExpression(IExpr expr)
        {
            var c = NewProductionsContext();

            Build(c, expr);

            //Add the productions from the context!
            foreach (var production in c.ActiveProductions)
            {
                AddProduction(production);
            }
        }

        protected virtual void Build(ProductionBuildContext<TRule> c, IExpr expr)
        {
            if (expr.IsGroup)
            {
                var addAsProduction = expr.IsProduction;
                if (expr.IsAlteration)
                    addAsProduction = false;

                foreach (var subExpr in expr.Expressions)
                {
                    var subExprContext = NewProductionsContext();
                    Build(subExprContext, subExpr);

                    if (addAsProduction)
                    {
                        c.AppendAsProductions(subExprContext);
                    }
                    else
                    {
                        c.AppendAsAlerations(subExprContext);
                    }
                }
            }
            else
            {
                //This is a leaf expression - create the symbol for the expression - and add it to the ongoing production
                var symbol = CreateSymbolForLeafExpr(expr);
                c.AddSymbol(symbol);
            }
        }
        
        protected virtual ISymbol CreateSymbolForLeafExpr(IExpr expr)
        {
            var charTerm = expr as IPatternTerminalCharExpr;
            if (charTerm != null)
            {
                return new LexPatternTerminalCharSymbol(charTerm.Char);
            }

            var rangeTerm = expr as IPatternTerminalRangeExpr;
            if (rangeTerm != null)
            {
                return new LexPatternTerminalRangeSymbol(rangeTerm.FromChar, rangeTerm.ToChar);
            }

            throw new Exception($"Unsupported expression ot type '{expr.GetType().Name}'");
        }

        [DebuggerStepThrough]
        protected virtual ProductionBuildContext<TRule> NewProductionsContext()
        {
            return new ProductionBuildContext<TRule>(_this);
        }
    }

    public class ProductionBuildContext<TRule>
        where TRule : Rule<TRule>, new()
    {
        private TRule _rule;

        public ProductionBuildContext(TRule rule)
        {
            _rule = rule;
            ActiveProductions = new RapidList<Production<TRule>>();
        }
        
        public RapidList<Production<TRule>> ActiveProductions { get; private set; }
        
        public void AddSymbol(ISymbol symbol)
        {
            //Make sure there is an active production!
            if (ActiveProductions.Count == 0)
            {
                ActiveProductions.Add(_rule.CreateProduction());
            }

            foreach (var production in ActiveProductions)
            {
                production.AddSymbol(symbol);
            }
        }

        public void AppendAsProductions(ProductionBuildContext<TRule> other)
        {
            if (ActiveProductions.Count == 0)
            {
                ActiveProductions.Add(_rule.CreateProduction());
            }
            
            foreach (var production in ActiveProductions)
            {
                Production<TRule> initialProduction = null;

                var otherProductions = other.ActiveProductions.GetEnumerator();

                if (other.ActiveProductions.Count > 1)
                {
                    //Prepare a clone of the initial production
                    initialProduction = production.Clone();
                }

                if (otherProductions.MoveNext())
                {
                    //For the first production just append
                    foreach (var otherSymbol in otherProductions.Current.RhsSymbols)
                    {
                        production.AddSymbol(otherSymbol);
                    }
                }

                while (otherProductions.MoveNext())
                {
                    //For all remaining productions, we need to clone the active production!
                    var clonedActiveProduction = initialProduction.Clone();
                    foreach (var otherSymbol in otherProductions.Current.RhsSymbols)
                    {
                        clonedActiveProduction.AddSymbol(otherSymbol);
                    }
                    ActiveProductions.Add(clonedActiveProduction);
                }
            }
        }

        public void AppendAsAlerations(ProductionBuildContext<TRule> other)
        {
            foreach (var otherProduction in other.ActiveProductions)
            {
                ActiveProductions.Add(otherProduction);
            }
        }
    }
}
