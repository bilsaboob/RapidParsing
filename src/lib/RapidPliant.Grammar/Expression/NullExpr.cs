using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Util;

namespace RapidPliant.Grammar.Expression
{
    public class NullExpr : GrammarExpr
    {
        public NullExpr()
        {
        }
        
        protected override void _ToStringExpr(IText text)
        {
            text.Append("NULL");
        }
    }
}
