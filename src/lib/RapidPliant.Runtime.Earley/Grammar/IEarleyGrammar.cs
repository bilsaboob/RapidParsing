using System.Collections.Generic;
using RapidPliant.Runtime.Earley.Automata.Earley;
using RapidPliant.Runtime.Earley.Lexing;

namespace RapidPliant.Runtime.Earley.Grammar
{
    public interface IEarleyGrammar
    {
        IEarleyDfa EarleyDfa { get; }
        IEnumerable<IEarleyRuleDef> GetStartRules();
    }

    public interface IEarleyLexDef
    {
        int LocalIndex { get; set; }
        ILexemeFactory LexemeFactory { get; set; }
    }

    public interface IEarleyRuleDef
    {
        int LocalIndex { get; set; }

        IProduction[] Productions { get; }
    }
}