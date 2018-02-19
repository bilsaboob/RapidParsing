using System;
using System.Collections.Generic;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Lexer.Builder;

namespace RapidPliant.Parsing.RecursiveDescent.RapidBnf
{
    public class RbnfTokenType : TokenType<RbnfTokenType>
    {
        public RbnfTokenType(int id, string name, TokenCategory category = null)
            : base(id, name, category)
        {
        }
    }
    
    /// <summary>
    /// RapidBnf Lexer definition
    /// </summary>
    public partial class RapidBnfGrammar
    {
        /// <summary>
        /// Token categories
        /// </summary>
        public static class TC
        {
            public static readonly TokenCategory WHITESPACE = new TokenCategory("Whitespace", true);
            public static readonly TokenCategory SEPARATOR = new TokenCategory("Separator", false);
            public static readonly TokenCategory COMMENT = new TokenCategory("Comment", true);
            public static readonly TokenCategory IDENTIFIER = new TokenCategory("Identifier", false);
            public static readonly TokenCategory OPERATOR = new TokenCategory("Operator", false);
            public static readonly TokenCategory LITERAL = new TokenCategory("Literal", false);
        }
        
        /// <summary>
        /// Token types 
        /// </summary>
        public static class T
        {
            public static int id = 1;

            public static readonly RbnfTokenType WHITESPACE = new RbnfTokenType(id++, "WS", TC.WHITESPACE);

            public static readonly RbnfTokenType BLOCK_COMMENT = new RbnfTokenType(id++, "/*...*/", TC.COMMENT);

            public static readonly RbnfTokenType LINE_COMMENT = new RbnfTokenType(id++, "//...", TC.COMMENT);

            public static readonly RbnfTokenType IDENTIFIER = new RbnfTokenType(id++, "Identifier", TC.IDENTIFIER);

            public static readonly RbnfTokenType STRING_LITERAL = new RbnfTokenType(id++, "StringLiteral", TC.LITERAL);

            public static readonly RbnfTokenType CHAR_STRING_LITERAL = new RbnfTokenType(id++, "CharStringLiteral", TC.LITERAL);

            public static readonly RbnfTokenType NUMBER = new RbnfTokenType(id++, "Number", TC.LITERAL);

            public static readonly RbnfTokenType OP_EQUALS = new RbnfTokenType(id++, "=", TC.OPERATOR);

            public static readonly RbnfTokenType OP_OR = new RbnfTokenType(id++, "|", TC.OPERATOR);
            
            public static readonly RbnfTokenType SEMI = new RbnfTokenType(id++, ";", TC.SEPARATOR);

            public static readonly RbnfTokenType REGEX_LITERAL = new RbnfTokenType(id++, "/.../", TC.LITERAL);

            public static readonly RbnfTokenType LP = new RbnfTokenType(id++, "(", TC.OPERATOR);

            public static readonly RbnfTokenType RP = new RbnfTokenType(id++, ")", TC.OPERATOR);

            public static readonly RbnfTokenType STAR = new RbnfTokenType(id++, "*", TC.OPERATOR);

            public static readonly RbnfTokenType PLUS = new RbnfTokenType(id++, "+", TC.OPERATOR);

            public static readonly RbnfTokenType QUESTION = new RbnfTokenType(id++, "?", TC.OPERATOR);

            public static readonly RbnfTokenType DOT = new RbnfTokenType(id++, ".", TC.OPERATOR);
        }

        public static Lexer CreateLexer()
        {
            var b = new DfaLexerBuilder();

            // literals
            b.StringLiteral(T.STRING_LITERAL);
            b.CharStringLiteral(T.CHAR_STRING_LITERAL);
            b.IntegerOrDecimalNumberLiteral(T.NUMBER);

            //Note that "similar" must start with "shortest match" and end with "longest match"
            // /.../ is shorter than /*...*/ ... and hence must be before, or the longest match will never hit
            b.RangeLiteral("/", "/", T.REGEX_LITERAL);
            b.BlockComment(T.BLOCK_COMMENT);
            b.LineComment(T.LINE_COMMENT);

            // identifiers
            b.Identifier(T.IDENTIFIER);

            // symbols
            b.Pattern("=", T.OP_EQUALS);
            b.Pattern("\\|", T.OP_OR);
            b.Pattern(";", T.SEMI);
            b.Pattern("\\(", T.LP);
            b.Pattern("\\)", T.RP);
            b.Pattern("\\*", T.STAR);
            b.Pattern("\\+", T.PLUS);
            b.Pattern("\\?", T.QUESTION);
            b.Pattern("\\.", T.DOT);

            // mark ignored token types
            Ignore(T.LINE_COMMENT, T.BLOCK_COMMENT, T.WHITESPACE);

            // mark ignored token categories
            Ignore(TC.WHITESPACE, TC.COMMENT);

            var lexer = b.CreateLexer();
            return lexer;
        }

        private static void Ignore(params TokenType[] tokenTypes)
        {
            // mark each of the token types to be ignored
            foreach (var tt in tokenTypes)
            {
                tt.Ignore();
            }
        }

        private static void Ignore(params TokenCategory[] tokenCategories)
        {
            // mark each of the token categories
            foreach (var c in tokenCategories)
            {
                c.Ignore();
            }
        }
    }

    /// <summary>
    /// RapidBnf Parse utils definition
    /// </summary>
    public partial class RapidBnfGrammar
    {
        private static bool SkipUntilParsed(ParseContext c, Func<ParseContext, bool> parser)
        {
            try
            {
                while (true)
                {
                    if(!c.AdvanceToken()) break;

                    // no rule type... but enter a "fake context" for the recovery logic...
                    c.Enter();

                    // try parsing until the expected rule... and break at that
                    if (parser(c.New()))
                    {
                        c.Accept();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            if (c.Exit())
            {
                // always reset the tokens if successfully recovered
                c.ResetTokens();
                return true;
            }

            return false;
        }
    }
    
    /// <summary>
    /// RapidBnf Parser definition
    /// </summary>
    public partial class RapidBnfGrammar
    {
        /// <summary>
        /// Rule types
        /// </summary>
        public static class R
        {
            public static int id = 1;

            public class GrammarRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "Grammar";
                public GrammarRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new Grammar();
            }
            public static readonly GrammarRuleType GRAMMAR = new GrammarRuleType();

            public class TopStatementsRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "TopStatements";
                public TopStatementsRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new TopStatements();
            }
            public static readonly TopStatementsRuleType TOP_STATEMENTS = new TopStatementsRuleType();

            public class TopDeclarationRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "TopDeclaration";
                public TopDeclarationRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new TopDeclaration();
            }
            public static readonly TopDeclarationRuleType TOP_DECLARATION = new TopDeclarationRuleType();

            public class RuleDeclarationRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "RuleDeclaration";
                public RuleDeclarationRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new RuleDeclaration();
            }
            public static readonly RuleDeclarationRuleType RULE_DECLARATION = new RuleDeclarationRuleType();

            public class RuleDefinitionRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "RuleDefinition";
                public RuleDefinitionRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new RuleDefinition();
            }
            public static readonly RuleDefinitionRuleType RULE_DEFINITION = new RuleDefinitionRuleType();

            public class RuleExpressionsRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "RuleExpressions";
                public RuleExpressionsRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new RuleExpressions();
            }
            public static readonly RuleExpressionsRuleType RULE_EXPRESSIONS = new RuleExpressionsRuleType();

            public class RuleExpressionRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "RuleExpression";
                public RuleExpressionRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new RuleExpression();
            }
            public static readonly RuleExpressionRuleType RULE_EXPRESSION = new RuleExpressionRuleType();

            public class RegexExpressionRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "RegexExpression";
                public RegexExpressionRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new RegexExpression();
            }
            public static readonly RegexExpressionRuleType REGEX_EXPRESSION = new RegexExpressionRuleType();

            public class GroupExpressionRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "GroupExpression";
                public GroupExpressionRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new GroupExpression();
            }
            public static readonly GroupExpressionRuleType GROUP_EXPRESSION = new GroupExpressionRuleType();

            public class RuleExpressionBnfOpRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "RuleExpressionBnfOp";
                public RuleExpressionBnfOpRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new RuleExpressionOperator();
            }
            public static readonly RuleExpressionBnfOpRuleType RULE_EXPRESSION_BNF_OP = new RuleExpressionBnfOpRuleType();

            public class RuleExpressionPinOpRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "RuleExpressionPinOp";
                public RuleExpressionPinOpRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new RuleExpressionPinOperator();
            }
            public static readonly RuleExpressionPinOpRuleType RULE_EXPRESSION_PIN_OP = new RuleExpressionPinOpRuleType();

            public class RefExpressionRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "RefExpression";
                public RefExpressionRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new RefExpression();
            }
            public static readonly RefExpressionRuleType REF_EXPRESSION = new RefExpressionRuleType();

            public class SpellingExpressionRuleType : RuleType
            {
                public static readonly int ID = id++;
                public static readonly string NAME = "SpellingExpression";
                public SpellingExpressionRuleType() : base(ID, NAME) { }
                public override ParseNode CreateNode() => new SpellingExpression();
            }
            public static readonly SpellingExpressionRuleType SPELLING_EXPRESSION = new SpellingExpressionRuleType();
        }
        
        public static bool Parse(ParseContext c)
        {
            return ParseGrammar(c);
        }

        public class Grammar : ParseRuleNode
        {
            public Grammar() : base(R.GRAMMAR) {}
            
            public TopStatements TopStatements => GetChild(0) as TopStatements;
        }

        public static bool ParseGrammar(ParseContext c)
        {
            c.Enter(R.GRAMMAR);

            try
            {
                if(ParseTopStatements(c.New())) { c.Accept(); }
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class TopStatements : ParseRuleNode
        {
            public TopStatements() : base(R.TOP_STATEMENTS) { }

            public IList<TopDeclaration> TopDeclarations => (GetChild(0) as ParseListNode<TopDeclaration>)?.AsList();
        }

        public static bool ParseTopStatements(ParseContext c)
        {
            c.Enter(R.TOP_STATEMENTS);

            try
            {
                while (!c.IsAtEnd)
                {
                    if(ParseTopDeclaration(c.New())) { c.Accept(); }
                    else
                    {
                        // we try to recover by going to the next possible top declaration, that starts on a new line on the same indentation level
                        if (!SkipUntilParsed(c, ParseTopDeclaration))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class TopDeclaration : ParseRuleNode
        {
            public TopDeclaration() : base(R.TOP_DECLARATION) { }

            public RuleDeclaration RuleDeclaration => null;
        }
        
        public static bool ParseTopDeclaration(ParseContext c)
        {
            c.Enter(R.TOP_DECLARATION);

            try
            {
                if (!ParseRuleDeclaration(c.New())) goto Exit;
                c.Accept();
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class RuleDeclaration : ParseRuleNode
        {
            public RuleDeclaration() : base(R.RULE_DECLARATION) { }

            public IToken IDENTIFIER { get; set; }
            public IToken OP_EQUALS { get; set; }
            public RuleDefinition RuleDefinition { get; set; }
        }

        public static bool ParseRuleDeclaration(ParseContext c)
        {
            c.Enter(R.RULE_DECLARATION);

            try
            {
                if (!c.AdvanceToken(T.IDENTIFIER)) goto Exit;
                if (!c.AdvanceToken(T.OP_EQUALS)) goto Exit;
                c.Pin();
                if (!ParseRuleDefinition(c.New())) goto Exit;
                c.Accept();
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class RuleDefinition : ParseRuleNode
        {
            public RuleDefinition() : base(R.RULE_DEFINITION) { }

            public RuleExpressions RuleExpressions => null;
            public IToken SEMI => null;
        }

        public static bool ParseRuleDefinition(ParseContext c)
        {
            c.Enter(R.RULE_DEFINITION);

            try
            {
                if (!ParseRuleExpressions(c.New())) goto Exit;

                // accept if definition ends with semicolon
                if (!c.AdvanceToken(T.SEMI)) goto Exit;
                c.Accept();
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class RuleExpressions : ParseRuleNode
        {
            public RuleExpressions() : base(R.RULE_EXPRESSIONS) { }

            public IList<Node> Expressions => null;

            public class Node : AstNode
            {
                public RuleExpression RuleExpression => null;
                public IToken OP_OR => null;
            }
        }

        public static bool ParseRuleExpressions(ParseContext c)
        {
            c.Enter(R.RULE_EXPRESSIONS);

            try
            {
                // given at least one rule expression, we accept this as a list
                if (!ParseRuleExpression(c.New())) goto Exit;

                c.Accept();

                while (true)
                {
                    if (c.AdvanceToken(T.OP_OR)) continue;

                    // given at least one rule expression, we accept this as a list
                    if (!ParseRuleExpression(c.New())) break;
                }
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class RuleExpression : ParseRuleNode
        {
            public RuleExpression() : base(R.RULE_EXPRESSION) { }

            public RefExpression RefExpression => null;
            public RegexExpression RegexExpression => null;
            public SpellingExpression SpellingExpression => null;
            public GroupExpression GroupExpression => null;
            public RuleExpressionPinOperator PinOp => null;
            public RuleExpressionOperator BnfOp => null;
        }

        private static bool ParseRuleExpression(ParseContext c)
        {
            c.Enter(R.RULE_EXPRESSION);

            try
            {
                if (ParseRefExpression(c.New()))
                {
                    c.Accept(); goto ExitSuccess;
                }

                if (ParseSpellingExpression(c.New()))
                {
                    c.Accept(); goto ExitSuccess;
                }

                if (ParseRegexExpression(c.New()))
                {
                    c.Accept(); goto ExitSuccess;
                }

                if (ParseGroupExpression(c.New()))
                {
                    c.Accept(); goto ExitSuccess;
                }

                goto Exit;
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            // rule expression has been parsed... now try parsing the rule expression operator and pin operator
            ExitSuccess:
            ParseRuleEexpressionOperator(c.New());
            ParseRulePinOperator(c.New());

            Exit:
            return c.Exit();
        }

        public class RuleExpressionPinOperator : ParseRuleNode
        {
            public RuleExpressionPinOperator() : base(R.RULE_EXPRESSION_PIN_OP) { }

            public IToken DOT => null;
            public IToken NUMBER => null;
        }

        private static bool ParseRulePinOperator(ParseContext c)
        {
            c.Enter(R.RULE_EXPRESSION_PIN_OP);

            try
            {
                if (c.AdvanceToken(T.DOT)) c.Accept();

                // optionally advance over a number too
                c.AdvanceToken(T.NUMBER, true);
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class RuleExpressionOperator : ParseRuleNode
        {
            public RuleExpressionOperator() : base(R.RULE_EXPRESSION_BNF_OP) { }
            
            public IToken STAR => null;
            public IToken QUESTION => null;
            public IToken PLUS => null;
        }

        private static bool ParseRuleEexpressionOperator(ParseContext c)
        {
            c.Enter(R.RULE_EXPRESSION_BNF_OP);

            try
            {
                if (c.AdvanceToken(T.STAR))
                {
                    c.Accept();
                    goto Exit;
                }

                if (c.AdvanceToken(T.QUESTION))
                {
                    c.Accept();
                    goto Exit;
                }

                if (c.AdvanceToken(T.PLUS))
                {
                    c.Accept();
                    goto Exit;
                }
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class GroupExpression : ParseRuleNode
        {
            public GroupExpression() : base(R.GROUP_EXPRESSION) { }

            public IToken LP => null;
            public RuleExpressions RuleExpressions => null;
            public IToken RP => null;
        }

        private static bool ParseGroupExpression(ParseContext c)
        {
            c.Enter(R.GROUP_EXPRESSION);

            try
            {
                if (!c.AdvanceToken(T.LP)) goto Exit;
                if (!ParseRuleExpressions(c.New())) goto Exit;
                if (!c.AdvanceToken(T.RP)) goto Exit;
                
                c.Accept();
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class RegexExpression : ParseRuleNode
        {
            public RegexExpression() : base(R.REGEX_EXPRESSION) { }

            public IToken REGEX_LITERAL => null;
        }

        private static bool ParseRegexExpression(ParseContext c)
        {
            c.Enter(R.REGEX_EXPRESSION);

            try
            {
                if (!c.AdvanceToken(T.REGEX_LITERAL)) goto Exit;
                
                c.Accept();
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class RefExpression : ParseRuleNode
        {
            public RefExpression() : base(R.REF_EXPRESSION) { }

            public IToken IDENTIFIER => null;
        }

        private static bool ParseRefExpression(ParseContext c)
        {
            c.Enter(R.REF_EXPRESSION);

            try
            {
                if (!c.AdvanceToken(T.IDENTIFIER)) goto Exit;
                c.Accept();
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }

        public class SpellingExpression : ParseRuleNode
        {
            public SpellingExpression() : base(R.SPELLING_EXPRESSION) { }

            public IToken CHAR_STRING_LITERAL => null;
        }

        private static bool ParseSpellingExpression(ParseContext c)
        {
            c.Enter(R.SPELLING_EXPRESSION);

            try
            {
                if (!c.AdvanceToken(T.CHAR_STRING_LITERAL)) goto Exit;
                c.Accept();
            }
            catch (Exception ex)
            {
                c.Exception(ex);
            }

            Exit:
            return c.Exit();
        }
    }
}

