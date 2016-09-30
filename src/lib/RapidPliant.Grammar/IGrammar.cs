using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar
{
    public interface IGrammar
    {
        IEnumerable<ILexRule> GetLexRules();
    }

    public interface ILexRule
    {
        int LocalIndex { get; set; }
    }

    public interface IExprRule
    {
    }
}
