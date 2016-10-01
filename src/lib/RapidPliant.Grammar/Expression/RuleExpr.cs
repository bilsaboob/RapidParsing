using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Grammar.Expression
{
    public class RuleExpr : Expr
    {
        public static implicit operator RuleExpr(string ruleName)
        {
            return new RuleExpr(ruleName);
        }
        
        private Expr _ruleModelExpr;

        public RuleExpr(string ruleName)
        {
            Name = ruleName;
            IsBuilder = false;
        }

        public string Name { get; private set; }

        public Expr DefinitionExpr { get { return _ruleModelExpr; } }

        public IRuleModel RuleModel { get; set; }
        
        public void As(Expr expr)
        {
            _ruleModelExpr = expr;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (_ruleModelExpr != null)
            {
                _ruleModelExpr.ToString(sb);
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

    public interface IRuleModelBuilder
    {
    }

    public class RuleModelBuilder : IRuleModelBuilder
    {
    }
}
