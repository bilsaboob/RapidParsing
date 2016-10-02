using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public partial class RuleExpr : Expr, IRuleExpr
    {
        private Expr _defExpr;

        public RuleExpr()
        {
        }
        
        public Expr DefinitionExpr { get { return _defExpr; } }
        
        public void As(Expr expr)
        {
            _defExpr = expr;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (_defExpr != null)
            {
                _defExpr.ToString(sb);
            }
            else
            {
                ToString(sb);
            }
            return sb.ToString();
        }

        public override void ToString(StringBuilder sb)
        {
            if (string.IsNullOrEmpty(Name))
            {
                sb.Append("?UNKNOWN?");
            }
            else
            {
                sb.Append(Name);
            }
        }
    }

    public abstract partial class Rule : RuleExpr
    {
        public static implicit operator Rule(string ruleName)
        {
            return new DeclarationRuleExpr(ruleName);
        }
    }
    
    public class DeclarationRuleExpr : Rule, IExprDeclaration
    {
        public DeclarationRuleExpr(string name)
        {
            Name = name;
        }
    }
}
