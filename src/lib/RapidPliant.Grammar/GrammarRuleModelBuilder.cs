using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public class GrammarRuleModelBuilder<TGrammar> 
        where TGrammar : GrammarModel<TGrammar>
    {
        private GrammarModel<TGrammar> _grammar;

        public GrammarRuleModelBuilder(GrammarModel<TGrammar> grammar)
        {
            _grammar = grammar;
        }

        public IRuleModel BuildRuleModel(RuleExpr ruleExpr)
        {
            var model = new RuleModel();

            var defExpr = ruleExpr.DefinitionExpr;
            if (defExpr == null)
                return null;

            BuildModel(model, defExpr);

            return model;
        }

        private void BuildModel(RuleModel model, Expr expr)
        {
            var symbol = CreateSymbolModel(expr, true);
            if (symbol != null)
            {
                model.AddSymbol(symbol);
                return;
            }

            //No alterations - means it's an empty expression... lets skip those!
            var altsCount = expr.Alterations.Count;
            if(altsCount == 0)
                return;
            
            //Iterate the alterations and add as a production
            foreach (var alt in expr.Alterations)
            {
                //Don't use empty alterations!
                if (alt.Count == 0)
                    continue;

                model.StartNewProduction();
                
                var singleSymbol = GetSingleExpandedSymbol(alt);
                if (singleSymbol != null)
                {
                    model.AddSymbol(singleSymbol);
                    continue;
                }
                
                foreach (var altExpr in alt.Expressions)
                {
                    BuildModel(model, altExpr);
                }
            }
        }

        private ISymbolModel GetSingleExpandedSymbol(ExprAlteration alteration)
        {
            var expressions = alteration.Expressions;
            if (expressions.Count == 1)
            {
                var expr = expressions[0];
                if (expr.IsBuilder)
                {
                    if (expr.Alterations.Count == 0)
                    {
                        //Empty expression?... just skip those!
                        return null;
                    }
                    else if (expr.Alterations.Count == 1)
                    {
                        //Go deeper...
                        return GetSingleExpandedSymbol(expr.Alterations[0]);
                    }

                    return null;
                }
                return CreateSymbolModel(expr);
            }

            return null;
        }

        private ISymbolModel CreateSymbolModel(Expr expr, bool tryCreate = false)
        {
            var lexExpr = expr as LexExpr;
            if (lexExpr != null)
            {
                return GetOrCreateLexSymbolModel(lexExpr);
            }

            var ruleExpr = expr as RuleExpr;
            if (ruleExpr != null)
            {
                return GetOrCreateRuleSymbolModel(ruleExpr);
            }

            var nullExpr = expr as NullExpr;
            if (nullExpr != null)
            {
                return GetOrCreateNullSymbolModel(nullExpr);
            }

            if(!tryCreate)
                throw new Exception($"Unsupported expression type '{expr.GetType().Name}'");

            return null;
        }

        private ISymbolModel GetOrCreateRuleSymbolModel(RuleExpr ruleExpr)
        {
            return new RuleSymbolModel(ruleExpr.Name);
        }

        private ISymbolModel GetOrCreateNullSymbolModel(NullExpr nullExpr)
        {
            return new NullSymbolModel();
        }

        private ISymbolModel GetOrCreateLexSymbolModel(LexExpr lexExpr)
        {
            return new LexSymbolModel(lexExpr.Name);
        }
    }

    public class NullSymbolModel : INullSymbolModel
    {
    }

    public class RuleSymbolModel : IRuleSmbolModel
    {
        public RuleSymbolModel(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public IRuleModel RuleModel { get; private set; }
    }

    public class LexSymbolModel : ILexSymbolModel
    {
        public LexSymbolModel(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public ILexModel LexModel { get; private set; }
    }

    public class RuleModel : IRuleModel
    {
        private RuleProductionsModel _productionsModel;

        public RuleModel()
        {
            _productionsModel = new RuleProductionsModel(this);
        }

        public IProductionsModel Productions { get; }

        public RuleProductionModel StartNewProduction()
        {
            return _productionsModel.StartNewProduction();
        }

        public void AddSymbol(ISymbolModel symbol)
        {
            _productionsModel.AddSymbol(symbol);
        }
    }

    public class RuleProductionsModel : IProductionsModel
    {
        private List<IProductionModel> _productions;
        private IProductionModel[] _productionsCached;
        private RuleProductionModel _production;

        private RuleModel _ruleModel;

        public RuleProductionsModel(RuleModel ruleModel)
        {
            _ruleModel = ruleModel;
            _productions = new List<IProductionModel>();
        }

        public IProductionModel[] All
        {
            get
            {
                if (_productionsCached == null)
                {
                    _productionsCached = _productions.ToArray();
                }

                return _productionsCached;
            }
        }

        public int Count { get { return _productions.Count; } }

        public IProductionModel this[int index]
        {
            get { return _productions[index]; }
        }

        public IEnumerator<IProductionModel> GetEnumerator()
        {
            return _productions.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public RuleProductionModel StartNewProduction()
        {
            //No empty productions!
            if (_production != null && _production.Symbols.Count == 0)
                return _production;

            _production = new RuleProductionModel(_ruleModel);
            _productions.Add(_production);
            _productionsCached = null;

            return _production;
        }

        public void AddSymbol(ISymbolModel symbol)
        {
            EnsureProduction();

            //Add the symbol to the production!
            _production.AddSymbol(symbol);
        }

        private void EnsureProduction()
        {
            if (_production == null)
            {
                _production = new RuleProductionModel(_ruleModel);
            }
        }
    }

    public class RuleProductionModel : IProductionModel
    {
        private ProductionSymbolsModel _symbols;
        private RuleModel _ruleModel;

        public RuleProductionModel(RuleModel ruleModel)
        {
            _ruleModel = ruleModel;
            _symbols = new ProductionSymbolsModel();
        }

        public IRuleModel Rule { get { return _ruleModel; } }

        public IProductionSymbolsModel Symbols { get { return _symbols; } }

        public void AddSymbol(ISymbolModel symbol)
        {
            _symbols.Add(symbol);
        }
    }

    public class ProductionSymbolsModel : IProductionSymbolsModel
    {
        private List<ISymbolModel> _symbols;
        private ISymbolModel[] _symbolsCached;

        public ProductionSymbolsModel()
        {
            _symbols = new List<ISymbolModel>();
        }

        public ISymbolModel[] All
        {
            get
            {
                if (_symbolsCached == null)
                {
                    _symbolsCached = _symbols.ToArray();
                }

                return _symbolsCached;
            }
        }
        
        public int Count { get { return _symbols.Count; } }

        public ISymbolModel this[int index]
        {
            get { return _symbols[index]; }
        }

        public void Add(ISymbolModel symbol)
        {
            _symbols.Add(symbol);
            _symbolsCached = null;
        }

        public IEnumerator<ISymbolModel> GetEnumerator()
        {
            return _symbols.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}