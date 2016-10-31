using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Automata.Nfa;
using RapidPliant.Common.Parsing;
using RapidPliant.Common.Symbols;
using RapidPliant.Grammar;

namespace RapidPliant.Parsing.Automata.Nfa
{
    public interface IEarleyNfa : INfa
    {
    }

    public class EarleyNfa : RapidPliant.Automata.Nfa.Nfa, IEarleyNfa
    {
        public EarleyNfa(INfaState start, INfaState end)
            : base(start, end)
        {
        }

        public IDictionary<RuleID, object> UnresolvedRuleNfaReferences { get; set; }

        public override INfaGraph ToNfaGraph()
        {
            var graph = new EarleyNfaGraph(this);
            graph.EnsureCompiled();
            return graph;
        }
    }

    public interface IEarleyNfaGraph : INfaGraph
    {
        new IEarleyNfa Nfa { get; }
    }

    public class EarleyNfaGraph : NfaGraph, IEarleyNfaGraph
    {
        public EarleyNfaGraph(INfa nfa) : base(nfa)
        {
        }

        public new IEarleyNfa Nfa { get { return (IEarleyNfa)base.Nfa; } }
    }

    public interface IEarleyNfaTransition : INfaTransition
    {
    }

    public interface IEarleyNfaNullTransition : IEarleyNfaTransition, INfaNullTransition
    {
    }
    
    public interface IEarleyNfaLexTransition : IEarleyNfaTransition
    {
        ILexDef LexDef { get; }
    }

    public interface IEarleyNfaResolveRuleTransition : IEarleyNfaTransition
    {
        IRuleDef RuleDef { get; }
    }

    public abstract class EarleyNfaTransition : NfaTransition, IEarleyNfaTransition
    {
    }

    public class EarleyNfaLexTransition : EarleyNfaTransition, IEarleyNfaLexTransition
    {
        public EarleyNfaLexTransition(ILexDef lexDef)
        {
            LexDef = lexDef;
        }

        public ILexDef LexDef { get; protected set; }

        protected override string ToTransitionSymbolString()
        {
            if (LexDef == null)
                return "";

            return LexDef.Name;
        }
    }

    //Transition expected to be resolved with nfa
    public class EarleyNfaResolveRuleTransition : EarleyNfaTransition, IEarleyNfaResolveRuleTransition
    {
        public EarleyNfaResolveRuleTransition(IRuleDef ruleDef)
        {
            RuleDef = ruleDef;
        }

        public IRuleDef RuleDef { get; }

        protected override string ToTransitionSymbolString()
        {
            if (RuleDef == null)
                return "";

            return RuleDef.Name.ToString();
        }
    }

    public class EarleyNfaNullTransition : EarleyNfaTransition, IEarleyNfaNullTransition
    {
        protected override string ToTransitionArrowString()
        {
            return "=>";
        }
    }

    public interface IEarleyNfaRuleTransition : IEarleyNfaNullTransition
    {
        IRuleDef RuleDef { get; }
    }

    public abstract class EarleyNfaRuleTransition : EarleyNfaNullTransition, IEarleyNfaRuleTransition
    {
        public EarleyNfaRuleTransition(IRuleDef ruleDef)
        {
            RuleDef = ruleDef;
        }

        public IRuleDef RuleDef { get; }

        protected override string ToTransitionSymbolString()
        {
            if (RuleDef == null)
                return "";

            return RuleDef.Name.ToString();
        }
    }

    public interface IEarleyNfaRulePredictionTransition : IEarleyNfaRuleTransition
    {
    }
    
    public class EarleyNfaRulePredictionTransition : EarleyNfaRuleTransition, IEarleyNfaRulePredictionTransition
    {
        public EarleyNfaRulePredictionTransition(IRuleDef ruleDef)
            : base(ruleDef)
        {
        }
        
        protected override string ToTransitionSymbolString()
        {
            if (RuleDef == null)
                return "";

            return $"P:{RuleDef.Name.ToString()}";
        }
    }

    public interface IEarleyNfaRuleCompletionTransition : IEarleyNfaRuleTransition
    {
    }

    public class EarleyNfaRuleCompletionTransition : EarleyNfaRuleTransition, IEarleyNfaRuleCompletionTransition
    {
        public EarleyNfaRuleCompletionTransition(IRuleDef completedRuleDef)
            : base(completedRuleDef)
        {
        }

        protected override string ToTransitionSymbolString()
        {
            if (RuleDef == null)
                return "";

            return $"C:{RuleDef.Name.ToString()}";
        }
    }
}
