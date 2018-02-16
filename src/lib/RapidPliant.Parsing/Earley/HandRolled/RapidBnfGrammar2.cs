using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Lexer.Builder;

namespace RapidPliant.Parsing.Earley.HandRolled2
{
    public class RbnfTokenType : TokenType<RbnfTokenType>
    {
        public RbnfTokenType(int id, string name, TokenCategory category = null)
            : base(id, name, category)
        {
        }
    }

    public class RuleType : IGrammarElement
    {
        public RuleType(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }

        public override string ToString()
        {
            return $"{Id}:{Name}";
        }
    }

    public class ParseContext
    {
        private ParseState _state;
        private ParseContext _parent;
        private RuleType _ruleType;
        private bool _advancedAny;
        private object _initialTokenState;

        [DebuggerStepThrough]
        public ParseContext(ITokenStream tokens)
            : this(new ParseState(tokens, 0))
        {
        }

        [DebuggerStepThrough]
        public ParseContext(ParseState state)
        {
            _state = state;
        }

        [DebuggerStepThrough]
        private ParseContext(ParseState state, ParseContext parent)
            : this(state)
        {
            _parent = parent;
        }
        
        public bool HasParsed { get; protected set; }
        public bool HasPinned { get; protected set; }

        public bool IsAccepted { get; protected set; }

        public int IndentationAtEnter { get; protected set; }

        [DebuggerStepThrough]
        public ParseContext New()
        {
            return new ParseContext(_state, this);
        }

        public void Enter(RuleType ruleType = null)
        {
            _ruleType = ruleType;

            // cache the token state
            _initialTokenState = _state.Tokens.GetState();
        }
        
        public bool Exit(bool accept)
        {
            if (accept) IsAccepted = true;
            return Exit();
        }
        
        public bool Exit()
        {
            var isAccepted = IsAccepted;

            // reset the tokens state back to what it was at the beginning
            if (!isAccepted)
            {
                Tokens.Reset(_initialTokenState);
            }

            return isAccepted;
        }

        public void ResetTokens()
        {
            Tokens.Reset(_initialTokenState);
        }

        #region token helpers
        private TokenType _tt;
        public object TT => Token?.TokenType;
        public TokenType _TT
        {
            get
            {
                if (_tt == null)
                {
                    var tt = TT;
                    if (tt != null)
                    {
                        _tt = tt as TokenType;
                    }
                }

                return _tt;
            }
        }
        public IToken Token => _state.Tokens.Token;
        public ITokenStream Tokens => _state.Tokens;

        [DebuggerStepThrough]
        public ParseContext Start()
        {
            // start the parsing... ensures that the tokens state is started!
            if (!Tokens.IsAtEnd && Tokens.Token == null)
                Tokens.MoveNext();

            return this;
        }

        public bool AdvanceToken()
        {
            // can't advance if we are at end
            if (IsAtEnd) return false;

            if (!Tokens.MoveNext())
            {
                // failed to go to any next one?
                throw new InvalidOperationException("Failed to move to next token");
            }

            var token = Token;
            _tt = null;
            if (token != null && token.IsBadToken)
            {
                // we have encountered a bad token
                BadToken(token);
                // but continue, simply move to the next again
                return AdvanceToken();
            }

            _advancedAny = true;
            return true;
        }

        public bool AdvanceToken(TokenType tt, bool isOptional = false)
        {
            var match = TokenIs(tt);
            if (!match)
            {
                // if the expected token type is not an ignore token but the current token is one... we can skip all ignore tokens and try again
                if (!IsIgnore(tt))
                {
                    // skip over any ignore tokens
                    if (IsIgnore(_TT))
                    {
                        return SkipIgnoresAdvanceToken(tt, isOptional);
                    }
                }

                if (!isOptional)
                {
                    Expected(tt);
                    return false;
                }

                return true;
            }
            else
            {
                AdvanceToken();
                return true;
            }
        }

        private bool SkipIgnoresAdvanceToken(TokenType tt, bool isOptional = false)
        {
            var isIgnoreToken = IsIgnore(_TT);
            if (isIgnoreToken)
            {
                var skippedCount = 0;
                var beforeSkipState = Tokens.GetState();
                var advancedPastIngores = false;

                // keep skipping over ignore tokens
                while (isIgnoreToken)
                {
                    // skip over the ignore
                    advancedPastIngores = AdvanceToken();
                    if (!advancedPastIngores)
                        break;
                    
                    skippedCount++;

                    isIgnoreToken = IsIgnore(_TT);
                }

                // if we have skipped any, we can attempt a match
                if (advancedPastIngores)
                {
                    //now try to match the token again, we should not be on any ignore token
                    var match = TokenIs(tt);
                    if (match)
                    {
                        // if we now have a match, we can safely accept it and continue
                        AdvanceToken();
                        return true;
                    }
                    else
                    {
                        // we still have no match, so perhaps we need to restore, if we have skipped any
                        if (skippedCount > 0)
                        {
                            //restore to before we started skipping
                            Tokens.Reset(beforeSkipState);
                        }

                        if (!isOptional)
                        {
                            Expected(tt);
                            return false;
                        }

                        return true;
                    }
                }
            }
            
            return false;
        }

        private bool IsIgnore(TokenType tt)
        {
            if (tt == null) return false;

            if (tt.Ignored)
                return true;

            if (tt.Category != null && tt.Category.Ignored)
                return true;

            return false;
        }

        public bool AdvanceTokenUntil(TokenType tt)
        {
            while (!TokenIs(tt))
            {
                if (!AdvanceToken())
                    return false;
            }

            AdvanceToken();

            return true;
        }
        
        protected bool TokenIs(TokenType tt)
        {
            return TT == tt;
        }

        protected bool TokenIsAny(params TokenType[] tts)
        {
            return false;
        }

        #endregion

        #region state helpers
        public bool IsAtEnd => _state.Tokens?.IsAtEnd ?? true;

        public ParseContext Accept()
        {
            IsAccepted = true;
            return this;
        }

        public ParseContext Pin()
        {
            HasPinned = true;
            return this;
        }
        
        #endregion

        #region errors
        public void Exception(Exception ex)
        {
            // exception during parsing...
        }

        public ParseContext BadToken(IToken token)
        {
            // bad token encountered... log this...
            return this;
        }

        public ParseContext Error(string message)
        {
            //There was en error!
            return this;
        }

        public ParseContext Expected(TokenType tt)
        {
            return this;
        }
        #endregion

        #region state
        public class ParseState
        {
            public ParseState(ITokenStream tokens, int index)
            {
                Tokens = tokens;
                Index = index;
            }

            public int Index { get; private set; }
            public ITokenStream Tokens { get; private set; }
        }
        #endregion
    }

    public partial class RapidBnfGrammar
    {
        public static class TC
        {
            public static readonly TokenCategory WHITESPACE = new TokenCategory("Whitespace", true);
            public static readonly TokenCategory SEPARATOR = new TokenCategory("Separator", false);
            public static readonly TokenCategory COMMENT = new TokenCategory("Comment", true);
            public static readonly TokenCategory IDENTIFIER = new TokenCategory("Identifier", false);
            public static readonly TokenCategory OPERATOR = new TokenCategory("Operator", false);
            public static readonly TokenCategory LITERAL = new TokenCategory("Literal", false);
        }
        
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

    public partial class RapidBnfGrammar
    {
        public static class R
        {
            public static int id = 1;

            public static readonly RuleType GRAMMAR = new RuleType(id++, "Grammar");
            public static readonly RuleType TOP_STATEMENTS = new RuleType(id++, "TopStatements");
            public static readonly RuleType TOP_DECLARATION = new RuleType(id++, "TopDeclaration");
            public static readonly RuleType RULE_DECLARATION = new RuleType(id++, "RuleDeclaration");
            public static readonly RuleType RULE_DEFINITION = new RuleType(id++, "RuleDefinition");
            public static readonly RuleType RULE_EXPRESSIONS = new RuleType(id++, "RuleExpressions");
            public static readonly RuleType RULE_EXPRESSION = new RuleType(id++, "RuleExpression");
            public static readonly RuleType REGEX_EXPRESSION = new RuleType(id++, "RegexExpression");
            public static readonly RuleType GROUP_EXPRESSION = new RuleType(id++, "GroupExpression");
            public static readonly RuleType RULE_EXPRESSION_BNF_OP = new RuleType(id++, "RuleExpressionBnfOp");
            public static readonly RuleType RULE_EXPRESSION_PIN_OP = new RuleType(id++, "RuleExpressionPinOp");
            public static readonly RuleType REF_EXPRESSION = new RuleType(id++, "RefExpression");
            public static readonly RuleType SPELLING_EXPRESSION = new RuleType(id++, "SpellingExpression");
        }

        public static bool Parse(ParseContext c)
        {
            return ParseGrammar(c);
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
