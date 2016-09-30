using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Runtime.Earley.Lexing;

namespace RapidPliant.Runtime.Earley.Grammar
{
    public interface IEarleyLexRule
    {
        ILexemeFactory LexemeFactory { get; set; }
    }
}
