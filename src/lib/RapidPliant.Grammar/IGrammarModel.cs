using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Util;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public interface IGrammarModel
    {
        IEnumerable<ILexExpr> GetLexExpressions();
        IEnumerable<IRuleExpr> GetRuleExpressions();
    }

    public interface IExpr
    {
        string Name { get; }
    }

    public interface IExprDeclaration
    {
        string Name { get; }
    }

    public interface INullExpr : IExpr
    {
    }

    public interface IGroupExpr : IExpr
    {
        IExpr[] Expressions { get; }
    }

    public interface IAlterationExpr : IGroupExpr
    {
    }

    public interface IProductionExpr : IGroupExpr
    {
    }
    
    public interface ILexExpr : IExpr
    {
    }

    public interface IRuleExpr : IExpr
    {
    }

    public interface ILexPatternExpr : ILexExpr
    {
        string Pattern { get; }
    }

    public interface ILexTerminalExpr : ILexExpr
    {
        char Char { get; }
    }
    
    public interface ILexSpellingExpr : ILexExpr
    {
        string Spelling { get; }
    }
    
}
