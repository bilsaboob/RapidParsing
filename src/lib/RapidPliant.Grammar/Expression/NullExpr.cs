﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public class NullExpr : GrammarExpr
    {
        public NullExpr()
        {
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("NULL");
        }
    }
}
