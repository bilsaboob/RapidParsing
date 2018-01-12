using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Symbols;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Lexer.Builder;
using RapidPliant.Parsing.ParseTree;

namespace RapidPliant.Parsing.Earley.HandRolled
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

    public partial class ParseContext
    {
        public ParseNode ParseRoot { get; set; }
        public AstNode AstRoot { get; set; }
    }

    public static class RbnfParseContextExtensions
    {
        public static RapidBnfGrammar.GrammarRule.GrammarNode RbnfGrammarNode(this ParseContext context)
        {
            return context.AstRoot.GetNode<RapidBnfGrammar.GrammarRule.GrammarNode>(0);
        }
    }
    
    public partial class ParseContext
    {
        private Stack<ParseFrame> _frames;
        private ITokenStream _tokens;
        private bool _advancedAny;

        public ParseContext(ITokenStream tokens)
        {
            _frames = new Stack<ParseFrame>();
            _tokens = tokens;
        }

        public ParseFrame Frame { get; private set; }

        public object TT
        {
            get
            {
                if (!_advancedAny) AdvanceToken();
                return _tokens.Token?.TokenType;
            }
        }

        public bool IsAtEnd => _tokens.IsAtEnd;

        public void AdvanceToken()
        {
            // can't advance if we are at end
            if(IsAtEnd) return;

            if (!_tokens.MoveNext())
            {
                // failed to go to any next one?
                throw new InvalidOperationException("Failed to move to next token");
            }

            var token = _tokens.Token;
            if (token != null && token.IsBadToken)
            {
                // we have encountered a bad token
                BadToken(token);
                // but continue, simply move to the next again
                AdvanceToken();
                return;
            }

            _advancedAny = true;
        }

        private void BadToken(IToken token)
        {
            // bad token encountered... log this...
        }

        public void Expected(TokenType tt)
        {
            // logs the expected token type at the current location in this current frame
            Frame.Expected(tt);
        }

        public void Expected(RuleType rt)
        {
            //TODO: should not use string... costly to pass around... better a calculated enum
            // logs the expected rule type at the curren location in this current frame
            Frame.Expected(rt);
        }

        public void PushFrame(ParseRule rule)
        {
            if(Frame != null)
                _frames.Push(Frame);

            Frame = new ParseFrame();
            Frame.Rule = rule;
            Frame.TokenState = _tokens.GetState();
        }

        public void PopFrame(bool resetTokens)
        {
            if (Frame == null)
                throw new InvalidOperationException("No frame on stack");

            // when resetting state, use the state when entered the frame
            var tokenStateWhenEntered = Frame.TokenState;

            if (_frames.Count == 0)
            {
                // reset empty frame
                Frame = null;
            }
            else
            {
                Frame = _frames.Pop();
            }

            // continue from the previous token state
            if (resetTokens)
            {
                _tokens.Reset(tokenStateWhenEntered);
            }
        }

        public class ParseFrame
        {
            private List<IGrammarElement> _expectations;

            public ParseFrame()
            {
            }

            public object TokenState { get; set; }
            public ParseRule Rule { get; set; }

            public IEnumerable<IGrammarElement> Expectations => _expectations;

            public void Expected(IGrammarElement elem)
            {
                if (_expectations == null)
                {
                    _expectations = new List<IGrammarElement>();
                }

                _expectations.Add(elem);
            }
        }
    }

    public class RuleContext
    {
        public RuleType Type { get; set; }
    }

    public class ParseRule : RuleContext
    {
        public bool Parse(ParseContext context)
        {
            Enter(context);
            try
            {
                Parse();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Exit();
            }

            return IsAccepted;
        }

        private bool _isAccepted;

        public ParseRule()
        {
        }
        
        public ParseContext Context { get; set; }
        public bool IsAccepted { get; set; }

        public int IndentAtEnter { get; set; }

        public bool HasParsed { get; set; }
        public bool HasPinned { get; set; }

        protected ParseContext c => Context;

        public virtual void Enter(ParseContext context)
        {
            Context = context;

            HasParsed = false;
            HasPinned = false;
            _isAccepted = false;

            Context.PushFrame(this);
        }

        protected virtual void Parse()
        {
            // override to parse
        }

        protected void Accept()
        {
            _isAccepted = true;
        }

        protected void Pin()
        {
            HasPinned = true;
        }

        public void Exit()
        {
            IsAccepted = HasPinned || _isAccepted;
            var resetTokens = !IsAccepted;
            Context.PopFrame(resetTokens);
        }

        #region helper utils
        public bool IsAtEnd => Context.IsAtEnd;

        protected bool AdvanceToken(TokenType tt)
        {
            if (!TokenIs(tt))
            {
                Context.Expected(tt);
                return false;
            }
            else
            {
                Context.AdvanceToken();
            }

            return true;
        }

        protected bool AdvanceAnyToken(TokenType tt1, TokenType tt2)
        {
            // one of the following should be advanced
            if (AdvanceToken(tt1)) return true;
            if (AdvanceToken(tt2)) return true;

            return false;
        }

        protected bool TokenIs(TokenType tt)
        {
            return Context.TT == tt;
        }

        protected bool TokenIsAny(params TokenType[] tt)
        {
            return false;
        }
        #endregion
    }

    public class GrammarParseRule : ParseRule
    {
        public GrammarParseRule()
        {
        }

        #region Parse helpers
        protected bool Parse(ParseRule rule)
        {
            if (!rule.Parse(Context))
            {
                // log expectation 
                Context.Expected(rule.Type);
                return false;
            }

            return true;
        }
        #endregion

        #region Recovery helpers
        protected void ContinueWithParentOf(ParseRule rule)
        {
            // unwind the stack until the parent of the specified rule and continue with that
        }

        protected bool SkipUntilParsed(ParseRule rule, int startAtIndent, bool continueWithParent = true)
        {
            // skip until we have an indentation of "startAtIndent"
            
            // now try parsing the given rule

            // if failed... try finding the next location with the given indentation => thus on a newline...

            // continue until successful
            ContinueWithParentOf(rule);

            return false;
        }

        protected void SkipUntilParsed(ParseRule rule, bool continueWithParent = true)
        {
            // skip until... expected rule ... then unwind the stack and continue with the parent of that rule
            ContinueWithParentOf(rule);
        }
        #endregion

    }

    public static class RapidBnfGrammar
    {
        public static bool ParseGrammar(ParseContext context)
        {
            return Grammar.Parse(context);
        }
        
        public static class T
        {
            public static int id = 1;

            public static readonly RbnfTokenType IDENTIFIER = new RbnfTokenType(id++, "Identifier");
            
            public static RbnfTokenType STRING_LITERAL = new RbnfTokenType(id++, "StringLiteral");

            public static RbnfTokenType CHAR_STRING_LITERAL = new RbnfTokenType(id++, "CharStringLiteral");

            public static readonly RbnfTokenType OP_EQUALS = new RbnfTokenType(id++, "=");

            public static readonly RbnfTokenType OP_OR = new RbnfTokenType(id++, "|");

            public static readonly RbnfTokenType SEMI = new RbnfTokenType(id++, ";");

            public static readonly RbnfTokenType SLASH = new RbnfTokenType(id++, "/");
        }

        public static Lexer CreateLexer()
        {
            var b = new DfaLexerBuilder();

            b.CharStringLiteral(T.CHAR_STRING_LITERAL);
            b.Identifier(T.IDENTIFIER);
            //b.IntegerLiteral(T.CHAR_STRING_LITERAL);

            // Finish with overrides - the keywords and finally symbols
            //b.Pattern("public", null, "PUBLIC");
            //b.Pattern("static", null, "STATIC");

            //b.Pattern("{", null, "LB");
            //b.Pattern("}", null, "RB");

            b.Pattern("=", T.OP_EQUALS);
            b.Pattern("\\|", T.OP_OR);
            b.Pattern(";", T.SEMI);
            b.Pattern("/", T.SLASH);

            var lexer = b.CreateLexer();
            return lexer;
        }

        public static readonly GrammarRule Grammar = new GrammarRule();
        public class GrammarRule : GrammarParseRule
        {
            public class GrammarNode : AstNode
            {
                public TopStatementsRule.TopStatementsNode TopStatements => GetNode<TopStatementsRule.TopStatementsNode>(0);
            }

            protected override void Parse()
            {
                if(Parse(TopStatements)) { Accept(); }
            }
        }

        public static readonly TopStatementsRule TopStatements = new TopStatementsRule();
        public class TopStatementsRule : GrammarParseRule
        {
            public class TopStatementsNode : AstNode
            {
                public IReadOnlyList<TopDeclarationRule.TopDeclarationNode> TopDeclarations => GetNodes<TopDeclarationRule.TopDeclarationNode>(0);
            }

            protected override void Parse()
            {
                while (!IsAtEnd)
                {
                    if(!Parse(TopDeclaration))
                    {
                        // we try to recover by going to the next possible top declaration, that starts on a new line on the same indentation level
                        if (SkipUntilParsed(TopDeclaration, IndentAtEnter)) { Accept(); }
                        return;
                    }
                }

                Accept();
            }
        }

        public static readonly TopDeclarationRule TopDeclaration = new TopDeclarationRule();
        public class TopDeclarationRule : GrammarParseRule
        {
            public class TopDeclarationNode : AstNode
            {
                public RuleDeclarationRule.RuleDeclarationNode RuleDeclaration => GetNode<RuleDeclarationRule.RuleDeclarationNode>(0);
            }

            protected override void Parse()
            {
                if (Parse(RuleDeclaration)) { Accept();}
            }
        }

        public static readonly RuleDeclarationRule RuleDeclaration = new RuleDeclarationRule();
        public class RuleDeclarationRule : GrammarParseRule
        {
            public class RuleDeclarationNode : AstNode
            {
                public ITerminalParseNode IDENTIFIER => GetTerminal(0, T.IDENTIFIER);

                public ITerminalParseNode OP_EQUALS => GetTerminal(1, T.IDENTIFIER);

                public RuleDefinitionRule.RuleDefinitionNode RuleDefinition => GetNode<RuleDefinitionRule.RuleDefinitionNode>(2);
            }

            protected override void Parse()
            {
                if (!AdvanceToken(T.IDENTIFIER)) return;
                if (!AdvanceToken(T.OP_EQUALS)) return;
                Pin();
                if (!Parse(RuleDefinition)) return;
                Accept();
            }
        }

        public static readonly RuleDefinitionRule RuleDefinition = new RuleDefinitionRule();
        public class RuleDefinitionRule : GrammarParseRule
        {
            public class RuleDefinitionNode : AstNode
            {
                public RuleExpressionsRule.RuleExpressionsNode RuleExpressions => GetNode<RuleExpressionsRule.RuleExpressionsNode>(0);
                public ITerminalParseNode OP_OR => GetTerminal(1, T.SEMI);
            }

            protected override void Parse()
            {
                if (!Parse(RuleExpressions)) return;

                // accept if definition ends with semicolon
                if(AdvanceToken(T.SEMI)) { Accept(); }
            }
        }

        public static readonly RuleExpressionsRule RuleExpressions = new RuleExpressionsRule();
        public class RuleExpressionsRule : GrammarParseRule
        {
            public class RuleExpressionsNode : AstNode
            {
                public IReadOnlyList<RuleExpressionItem> RuleExpressions => GetNodes<RuleExpressionItem>(0);
            }

            public class RuleExpressionItem : AstNode
            {
                public ITerminalParseNode OP_OR => GetTerminal(0, T.OP_OR);
                public RuleExpressionRule.RuleExpressionNode RuleExpression => GetNode<RuleExpressionRule.RuleExpressionNode>(1);
            }

            protected override void Parse()
            {
                // given at least one rule expression, we accept this as a list
                if (!Parse(RapidBnfGrammar.RuleExpression)) { return; }

                Accept();

                while (true)
                {
                    if (AdvanceToken(T.OP_OR)) continue;

                    // given at least one rule expression, we accept this as a list
                    if (!Parse(RapidBnfGrammar.RuleExpression)) break;
                }
            }
        }

        public static readonly RuleExpressionRule RuleExpression = new RuleExpressionRule();
        public class RuleExpressionRule : GrammarParseRule
        {
            public class RuleExpressionNode : AstNode
            {
                public RegexExpressionRule.RegexExpresssionNode RegexExpression => GetNode<RegexExpressionRule.RegexExpresssionNode>(0);
                public RefExpressionRule.RefExpressionNode RefExpression => GetNode<RefExpressionRule.RefExpressionNode>(0);
                public SpellingExpressionRule.SpellingExpressionNode SpellingExpression => GetNode<SpellingExpressionRule.SpellingExpressionNode>(0);
            }

            protected override void Parse()
            {
                // parse an expression
                if (Parse(RegexExpression)) { Accept(); return; }
                if (Parse(RefExpression)) { Accept(); return; }
                if (Parse(SpellingExpression)) { Accept(); return; }
            }
        }

        public static readonly RefExpressionRule RefExpression = new RefExpressionRule();
        public class RefExpressionRule : GrammarParseRule
        {
            public class RefExpressionNode : AstNode
            {
                public ITerminalParseNode IDENTIFIER_0 { get; set; }
                public ITerminalParseNode SLASH_0 { get; set; }
                public ITerminalParseNode IDENTIFIER_1 { get; set; }
                public ITerminalParseNode SLASH_1 { get; set; }
            }

            protected override void Parse()
            {
                /*if(AdvanceToken(T.IDENTIFIER))
                    Accept();*/

                if (!AdvanceToken(T.IDENTIFIER)) return;

                Accept();
            }
        }

        public static readonly RegexExpressionRule RegexExpression = new RegexExpressionRule();
        public class RegexExpressionRule : GrammarParseRule
        {
            public class RegexExpresssionNode : AstNode
            {
                public ITerminalParseNode IDENTIFIER_0 { get; set; }
                public ITerminalParseNode SLASH_0 { get; set; }
                public ITerminalParseNode IDENTIFIER_1 { get; set; }
                public ITerminalParseNode SLASH_1 { get; set; }
            }

            protected override void Parse()
            {
                if (!AdvanceToken(T.SLASH)) return;

                // keep eating until a slash
                //var start = TokenLocation;
                //SkipUntilToken(T.SLASH);
                //var end = TokenLocation;

                // parse regex grammar as a sub range
                //RegexGrammar.Parse(Context.New(new RangeBufferTokenStream(start, end)));

                //if (!Parse(RegexGrammar.Regex, until: T.SLASH)) return;

                if (!AdvanceToken(T.SLASH)) return;

                Accept();
            }
        }

        public static readonly SpellingExpressionRule SpellingExpression = new SpellingExpressionRule();
        public class SpellingExpressionRule : GrammarParseRule
        {
            public class SpellingExpressionNode : AstNode
            {
            }

            protected override void Parse()
            {
                if(AdvanceToken(T.CHAR_STRING_LITERAL))
                    Accept();
            }
        }
        
    }
}
