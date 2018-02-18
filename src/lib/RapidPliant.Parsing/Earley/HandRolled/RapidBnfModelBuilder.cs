using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Parsing.Earley.HandRolled2;

namespace RapidPliant.Parsing.Earley.HandRolled
{
    public class RapidBnfModelBuilder
    {
        public RapidBnfModelBuilder()
        {
        }

        public void Build(RapidBnfGrammar.Grammar g)
        {
            // collect the lex rules
            foreach (var topStat in g.TopStatements.TopDeclarations)
            {
                var ruleDecl = topStat.RuleDeclaration;
                if (ruleDecl != null)
                {
                    var rule = new RbnfRule(ruleDecl);
                }
            }
        }

        #region Helpers
        private string GetText(IToken token)
        {
            return "";
        }
        #endregion
    }

    public class RbnfRule
    {
        public RbnfRule(RapidBnfGrammar.RuleDeclaration ruleDecl)
        {
            RuleDecl = ruleDecl;
        }
        
        public RapidBnfGrammar.RuleDeclaration RuleDecl { get; set; }
        public string Name => GetText(RuleDecl.IDENTIFIER);
        public IEnumerable<RapidBnfGrammar.RuleExpression> RuleExpressions => RuleDecl.RuleDefinition.RuleExpressions.Expressions.Select(e => e.RuleExpression);

        public bool IsLexRule { get; private set; }
        public bool IsLexSpelling { get; private set; }

        public bool IsParseRule { get; private set; }

        public void Process()
        {
            var isLexRule = true;
            var isLexSpellingRule = true;
            
            foreach (var expr in RuleExpressions)
            {
                var refExpr = expr.RefExpression;
                if (refExpr != null)
                {
                    // it's a ref to another rule ... we would need to check if that rule in turn is a parse or lex rule...
                    var refRuleName = GetText(refExpr);
                    var rule = GetRule(refRuleName);
                    if (rule == null)
                        continue;

                    if (rule.IsParseRule)
                    {
                        isLexRule = false;
                    }

                    // references to other rules... not a spelling rule
                    isLexSpellingRule = false;
                    continue;
                }

                var spellingExpr = expr.SpellingExpression;
                if (spellingExpr != null)
                {
                    // we have a lex spelling...
                    continue;
                }

                var regexExpr = expr.RegexExpression;
                if (regexExpr != null)
                {
                    // we have a regex expr...
                    continue;
                }

                var groupExpr = expr.GroupExpression;
                if (groupExpr != null)
                {
                    // we have a group... and should evaluate that too...
                    continue;
                }
            }

            IsLexRule = isLexRule;
            IsParseRule = !IsLexRule;
        }

        #region Helpers
        private RbnfRule GetRule(string ruleName)
        {
            return null;
        }

        private string GetText(IParseNode parseNode)
        {
            return "";
        }

        private string GetText(IToken token)
        {
            return "";
        }
        #endregion
    }
}
