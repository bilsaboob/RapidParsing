using System;
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
        IEnumerable<ILexModel> GetLexModels();
        IEnumerable<IRuleModel> GetRuleModels();
    }

    public interface ISymbolModel
    {
    }

    public interface IRuleSmbolModel : ISymbolModel
    {
        string Name { get; }
        IRuleModel RuleModel { get; }
    }

    public interface ILexSymbolModel : ISymbolModel
    {
        string Name { get; }
        ILexModel LexModel { get; }
    }

    public interface INullSymbolModel : ISymbolModel
    {
    }
    
    public interface IRuleModel
    {
        IProductionsModel Productions { get; }
    }

    public interface IProductionModel
    {
        IRuleModel Rule { get; }

        IProductionSymbolsModel Symbols { get; }
    }

    public interface IProductionsModel : IEnumerable<IProductionModel>
    {
        int Count { get; }

        IProductionModel this[int index] { get; }
    }
    
    public interface IProductionSymbolsModel : IEnumerable<ISymbolModel>
    {
        int Count { get; }

        ISymbolModel this[int index] { get; }
    }

    public interface ILexModel
    {
    }

    public abstract class LexModel : ILexModel
    {
    }

    public interface ILexPatternModel : ILexModel
    {
        string Pattern { get; }
    }

    public class LexPatternModel : LexModel, ILexPatternModel
    {
        public LexPatternModel(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; private set; }

        public override string ToString()
        {
            return "<" + Pattern + ">";
        }
    }

    public interface ILexTerminalModel : ILexModel
    {
        char Char { get; }
    }

    public class LexTerminalModel : LexModel, ILexTerminalModel
    {
        public LexTerminalModel(char c)
        {
            Char = c;
        }

        public char Char { get; private set; }

        public override string ToString()
        {
            return "'" + Char.ToString() + "'";
        }
    }

    public interface ILexSpellingModel : ILexModel
    {
        string Spelling { get; }
    }

    public class LexSpellingModel : LexModel, ILexSpellingModel
    {
        public LexSpellingModel(string spelling)
        {
            Spelling = spelling;
        }

        public string Spelling { get; private set; }

        public override string ToString()
        {
            return "\"" + Spelling + "\"";
        }
    }

    
}
