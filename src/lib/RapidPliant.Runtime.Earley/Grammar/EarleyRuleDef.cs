using System.Collections.Generic;
using RapidPliant.Common.Expression;
using RapidPliant.Grammar;

namespace RapidPliant.Runtime.Earley.Grammar
{
    public partial class EarleyRuleDef : IEarleyRuleDef, IProductionLhs
    {
        public EarleyRuleDef(IRuleDef ruleDef)
        {
            RuleDef = ruleDef;

            Id = ruleDef.Name;
            Name = ruleDef.Name;
        }

        public object Id { get; private set; }

        public string Name { get; private set; }

        public IRuleDef RuleDef { get; set; }

        public int LocalIndex { get; set; }

        public IProduction[] Productions { get; private set; }

        public void Build()
        {
            if (Productions == null)
            {
                Productions = BuildProductions().ToArray();
            }
        }
    }

    public partial class EarleyRuleDef : IEarleyRuleDef, IProductionLhs
    {
        protected List<Production> BuildProductions()
        {
            var productions = BuildForExpr(RuleDef.Expression);
            return productions;
        }

        private List<Production> BuildForExpr(IExpr expr, List<Production> prevProductions = null)
        {
            if(prevProductions == null)
                prevProductions = new List<Production>();
            
            var productions = new List<Production>(prevProductions);

            if (expr.IsGroup)
            {
                foreach (var childExpr in expr.Expressions)
                {
                    var childProductions = BuildForExpr(childExpr);

                    if (prevProductions.Count > 0)
                    {
                        foreach (var prevProduction in prevProductions)
                        {
                            foreach (var childProduction in childProductions)
                            {
                                var prevProductionWithChildContinuation = prevProduction.Clone();
                                prevProductionWithChildContinuation.Rhs.AddAll(childProduction);
                                productions.Add(prevProductionWithChildContinuation);
                            }
                        }
                    }
                    else
                    {
                        productions.AddRange(childProductions);
                    }
                }

                return productions;
            }

            var production = new Production(this);
            production.Rhs.Add(new Symbol(expr.Name));
            productions.Add(production);

            return productions;
        }
    }

    internal class Symbol : ISymbol
    {
        public Symbol(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}