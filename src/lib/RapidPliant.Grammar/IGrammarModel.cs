using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;
using RapidPliant.Common.Util;
using RapidPliant.Grammar.Definitions;
using RapidPliant.Grammar.Expression;

namespace RapidPliant.Grammar
{
    public interface IGrammarModel
    {
        IEnumerable<ILexDef> GetLexDefinitions();
        IEnumerable<IRuleDef> GetRuleDefinitions();
        IEnumerable<IRuleDef> GetStartRules();
    }

    public interface IGrammarDef
    {
        string Name { get; }
    }

    public abstract class GrammarDef : IGrammarDef
    {
        public string Name { get; set; }

        #region helpers
        public static RuleRefExpr RuleRef(RuleDef ruleDef)
        {
            return new RuleRefExpr(ruleDef);
        }

        public static LexRefExpr LexRef(LexDef lexDef)
        {
            return new LexRefExpr(lexDef);
        }

        public static ILexModel LexModelForString(string str)
        {
            ILexModel lexModel;
            if (str.Length == 1)
            {
                lexModel = new LexTerminalModel(str[0]);
            }
            else
            {
                lexModel = new LexSpellingModel(str);
            }
            return lexModel;
        }

        public static LexRefExpr InPlaceLexRef(string spelling)
        {
            return new LexRefExpr(InPlaceLexDef(spelling));
        }

        public static LexRefExpr InPlaceLexRef(char character)
        {
            return new LexRefExpr(InPlaceLexDef(character));
        }

        public static InPlaceLexDef InPlaceLexDef(string spelling)
        {
            return new InPlaceLexDef(LexModelForString(spelling));
        }

        public static InPlaceLexDef InPlaceLexDef(char character)
        {
            return new InPlaceLexDef(new LexTerminalModel(character));
        }
        #endregion
    }

    public interface ILexDef : IGrammarDef
    {
        ILexModel LexModel { get; }
    }
    
    public interface IRuleDef : IGrammarDef
    {
        IExpr Expression { get; }
    }
    
    public interface ILexModel
    {
    }

    public interface IRuleModel
    {
    }

    public interface ILexPatternModel : ILexModel
    {
        string Pattern { get; }
    }

    public interface ILexTerminalModel : ILexModel
    {
        char Char { get; }
    }
    
    public interface ILexSpellingModel : ILexModel
    {
        string Spelling { get; }
    }
    
}
