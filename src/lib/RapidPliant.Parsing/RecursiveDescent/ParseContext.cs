using System;
using System.Diagnostics;
using RapidPliant.Lexing.Lexer;

namespace RapidPliant.Parsing.RecursiveDescent
{
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

    public interface IParseNode
    {
    }

    public class ParseNode : IParseNode
    {
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
}