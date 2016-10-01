using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public class LexExpr : Expr
    {
        public static implicit operator LexExpr(string name)
        {
            return new LexExpr(name);
        }

        public LexExpr(string name)
            : this(name, null)
        {
        }

        public LexExpr(ILexModel lexModel)
            : this(null, lexModel)
        {
        }

        public LexExpr(string name, ILexModel lexModel)
        {
            Name = name;
            LexModel = lexModel;
            IsBuilder = false;
        }

        public string Name { get; set; }

        public ILexModel LexModel { get; private set; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public override void ToString(StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                sb.Append(Name);
            }
            else
            {
                sb.Append(LexModel.ToString());
            }
        }
    }
}
