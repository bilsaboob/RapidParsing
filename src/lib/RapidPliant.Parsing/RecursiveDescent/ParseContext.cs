using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Util;

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

        public virtual ParseNode CreateNode()
        {
            return null;
        }
    }

    public interface IAstNode
    {
        IToken GetToken(int index, TokenType tokenType = null);
    }

    public class AstNode : IAstNode
    {
        public AstNode()
        {
        }

        public AstNode(IParseNode node)
        {
            ParseNode = node;
        }

        public IParseNode ParseNode { get; protected set; }

        public IToken GetToken(int index, TokenType tokenType = null)
        {
            return null;
        }
    }

    public class ParseNodeListPool
    {
        private static readonly Queue<List<IParseNode>> _lists = new Queue<List<IParseNode>>();
        private static int _next = -1;

        public static List<IParseNode> Get()
        {
            /*if (_next == -1 || _lists.Count == 0)
            {
                return new List<ParseNode>();
            }
            else
            {
                var list = _lists[_next];
                if(list == null)
                    return new List<ParseNode>();

                _lists[_next] = null;
                _next--;
                
                if (list.Count != 0)
                {
                    list.Clear();
                }

                return list;
            }*/

            if (_lists.Count == 0)
            {
                return new List<IParseNode>();
            }
            else
            {
                return _lists.Dequeue();
            }
        }

        public static void FreeAndClear(List<IParseNode> list)
        {
            if(list == null) return;

            list.Clear();

            /*var next = _next + 1;
            if (next == 0)
            {
                _lists.Add(list);
                _next = 0;
                return;
            }

            if (next == _lists.Count)
            {
                _lists.Add(list);
                ++_next;
                return;
            }

            while (_next >= _lists.Count)
            {
                _lists.Add(null);
            }

            _lists[++_next] = list;*/
            
            _lists.Enqueue(list);
        }
    }

    public class ParseTree : IDisposable
    {
        private List<IParseNode> _nodes;
        internal int _index;

        public ParseTree()
        {
            _nodes = ParseNodeListPool.Get();
        }

        public int Count => _nodes.Count;

        internal int AddParseNode(IParseNode child)
        {
            var index = _index;
            if (index == _nodes.Count)
            {
                _nodes.Add(child);
                ++_index;
            }
            else
            {
                _nodes[index] = child;
                ++_index;
            }

            return index;
        }

        public IParseNode GetParseNode(int offset)
        {
            return _nodes[offset];
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_nodes != null)
            {
                ParseNodeListPool.FreeAndClear(_nodes);
            }
        }
    }

    public interface IParseNode
    {
    }

    public class ParseNode : IParseNode
    {
        private ParseTree _tree;
        private int _index;
        
        public IParseNode GetChild(int index)
        {
            var childIndex = _index + index;
            return _tree.GetParseNode(childIndex);
        }

        public virtual void AddChild(ParseNode node)
        {
            // insert at the child index
            node._index = _tree.AddParseNode(node);
            node._tree = _tree;
        }

        public void RemoveChild(ParseNode parseNode)
        {
            // reset to before the child... we don't support removal in the "middle"
            var offset = parseNode._index - _index;
            _tree._index = _index + offset;
        }

        public void BindTree(ParseTree parseTree)
        {
            _tree = parseTree;
            _index = _tree.AddParseNode(this);
        }
    }

    public class ParseRuleNode : ParseNode
    {
        public ParseRuleNode(RuleType ruleType)
        {
            RuleType = ruleType;
        }

        public RuleType RuleType { get; protected set; }
    }

    public class ParseTokenNode : ParseNode
    {
        public ParseTokenNode(IToken token)
        {
            Token = token;
        }
        
        public IToken Token { get; protected set; }
    }

    public class ParseListNode<T> : ParseNode
        where T: ParseNode, new()
    {
        public List<T> AsList()
        {
            return null;
        }
    }

    public class ParseContext
    {
        private ParseContext _parent;
        private ParseNode _parseNode;

        private ParseState _state;
        private RuleType _ruleType;
        private bool _advancedAny;
        private object _initialTokenState;

        private bool _buildParseTree = true;

        [DebuggerStepThrough]
        public ParseContext(ITokenStream tokens)
            : this(new ParseState(new ParseTree(), tokens, 0))
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

        public ParseNode ParseNode => _parseNode;

        public ParseTree ParseTree => _state.ParseTree;

        [DebuggerStepThrough]
        public ParseContext New()
        {
            return new ParseContext(_state, this);
        }

        public void Enter(RuleType ruleType = null)
        {
            _ruleType = ruleType;

            // prepare the parse node for this
            if (_ruleType != null)
            {
                _parseNode = _ruleType.CreateNode();

                // add as a parse node
                if (_buildParseTree && _parseNode != null)
                {
                    if (_parent == null)
                    {
                        _parseNode.BindTree(_state.ParseTree);
                    }
                    else
                    {
                        // add this parse node to the parent
                        _parent?._parseNode?.AddChild(_parseNode);
                    }
                }
            }

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

            // add as a parse node
            if (_buildParseTree)
            {
                // rollback the added node if not accepted
                if (!isAccepted)
                {
                    // add this parse node to the parent
                    _parent?._parseNode?.RemoveChild(_parseNode);
                }
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

        public bool AdvanceToken(bool consume = true)
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
                return AdvanceToken(consume);
            }

            // if we should consume the token, add to the parse tree
            if (consume && _buildParseTree)
            {
                _parseNode?.AddChild(new ParseTokenNode(token));
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
                    // skip over the ignore, don't consume the token
                    advancedPastIngores = AdvanceToken(false);
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
            public ParseState(ParseTree parseTree, ITokenStream tokens, int index)
            {
                ParseTree = parseTree;
                Tokens = tokens;
                Index = index;
            }

            public ParseTree ParseTree { get; private set; }
            public int Index { get; private set; }
            public ITokenStream Tokens { get; private set; }
        }
        #endregion
    }
}