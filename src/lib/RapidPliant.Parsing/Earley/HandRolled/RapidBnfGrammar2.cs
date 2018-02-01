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
    public class RbnfTokenType : TokenType
    {
        public RbnfTokenType(int id, string name)
            : base(id, name)
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
        public object TT => Token?.TokenType;
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

        public bool AdvanceToken(TokenType tt)
        {
            if (!TokenIs(tt))
            {
                Expected(tt);
                return false;
            }
            else
            {
                AdvanceToken();
            }

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
        public static class T
        {
            public static int id = 1;

            public static readonly RbnfTokenType IDENTIFIER = new RbnfTokenType(id++, "Identifier");

            public static readonly RbnfTokenType STRING_LITERAL = new RbnfTokenType(id++, "StringLiteral");

            public static readonly RbnfTokenType CHAR_STRING_LITERAL = new RbnfTokenType(id++, "CharStringLiteral");

            public static readonly RbnfTokenType NUMBER = new RbnfTokenType(id++, "Number");

            public static readonly RbnfTokenType OP_EQUALS = new RbnfTokenType(id++, "=");

            public static readonly RbnfTokenType OP_OR = new RbnfTokenType(id++, "|");

            public static readonly RbnfTokenType SEMI = new RbnfTokenType(id++, ";");

            public static readonly RbnfTokenType SLASH = new RbnfTokenType(id++, "/");
        }

        public static Lexer CreateLexer()
        {
            var b = new DfaLexerBuilder();

            // literals
            b.StringLiteral(T.STRING_LITERAL);
            b.CharStringLiteral(T.CHAR_STRING_LITERAL);
            b.IntegerOrDecimalNumberLiteral(T.NUMBER);

            // identifiers
            b.Identifier(T.IDENTIFIER);

            // symbols
            b.Pattern("=", T.OP_EQUALS);
            b.Pattern("\\|", T.OP_OR);
            b.Pattern(";", T.SEMI);
            b.Pattern("/", T.SLASH);

            var lexer = b.CreateLexer();
            return lexer;
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
                {
                    if (!c.AdvanceToken(T.IDENTIFIER)) goto Exit;
                    if (!c.AdvanceToken(T.OP_EQUALS)) goto Exit;
                    c.Pin();
                    if (!ParseRuleDefinition(c.New())) goto Exit;
                    c.Accept();
                }
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
                if (ParseRegexExpression(c.New())) { c.Accept(); goto Exit; }
                if (ParseRefExpression(c.New())) { c.Accept(); goto Exit; }
                if (ParseSpellingExpression(c.New())) { c.Accept(); goto Exit; }
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
                if (!c.AdvanceToken(T.SLASH)) goto Exit;

                // keep eating until another slash... it's our regex expression tokens
                // now use the Regex parser with the tokens we just consumed

                if (!c.AdvanceToken(T.SLASH)) goto Exit;

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
