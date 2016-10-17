using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Expression;

namespace RapidPliant.Lexing.Pattern.Regex
{
    public class RegexPatternParser
    {
        public RegexPatternExpr Parse(TextReader input)
        {
            var c = new RegexParseContext(input);
            var expr = Parse(c);
            return expr;
        }

        private RegexPatternExpr Parse(RegexParseContext c, params char[] breakOnChars)
        {
            Parse(c, new List<char>(breakOnChars));
            return c.ToExpression();
        }

        private void ParseUntil(RegexParseContext c, params char[] breakOnChars)
        {
            Parse(c, new List<char>(breakOnChars));
        }

        private void Parse(RegexParseContext c, List<char> breakOnChars)
        {
            while (c.MoveNext())
            {
                RegexTerminalExpr termExpr;
                if (c.IsEscaped)
                {
                    //Just consume as a normal character!?
                    termExpr = CreateTerminalExpr(c);
                    termExpr.Options = ParseOptions(c);
                    c.AddExpr(termExpr);
                    continue;
                }

                if (breakOnChars != null)
                {
                    if (breakOnChars.Contains(c.Char))
                    {
                        break;
                    }
                }

                //Check for group
                if (c == '[')
                {
                    //Parse the group
                    var groupExpr = ParseGroup(c.New());
                    groupExpr.Options = ParseOptions(c);
                    c.AddExpr(groupExpr);
                    continue;
                }

                //Check for new block
                if (c == '(')
                {
                    var blockExpr = ParseBlock(c.New());
                    blockExpr.Options = ParseOptions(c);
                    //Explicit block expressions are not allowed to be simplified!
                    blockExpr.CanBeSimplified = false;
                    c.AddExpr(blockExpr);
                    continue;
                }

                //Check for alteration
                if (c == '|')
                {
                    //Ensure there is an alteration expression at the top!
                    c.AddNextAsAlteration();
                    continue;
                }

                //Create a term expression!
                termExpr = CreateTerminalExpr(c);
                termExpr.Options = ParseOptions(c);
                c.AddExpr(termExpr);
            }
        }

        private RegexPatternExpr ParseBlock(RegexParseContext c)
        {
            ParseUntil(c, ')');
            return c.ToExpression();
        }
        
        private RegexPatternExpr ParseAlias(RegexParseContext c)
        {
            return null;
        }

        private RegexPatternExpr ParseGroup(RegexParseContext c)
        {
            while (c.MoveNext())
            {
                if (c == ']')
                    break;

                if (c == '-')
                {
                    var rangeExpr = CreateRangeExpr(c);
                }
            }

            return null;
        }

        private ExprOptions ParseOptions(RegexParseContext c)
        {
            var options = new ExprOptions();
            options.MaxCount = int.MaxValue;

            var hasOptions = false;
            
            var ch = c.NextChar;
            if (ch == '?')
            {
                hasOptions = true;
                options.IsOptional = true;
            }
            else if (ch == '*')
            {
                hasOptions = true;
                options.IsMany = true;
                options.IsOptional = true;
            }
            else if (ch == '+')
            {
                hasOptions = true;
                options.IsMany = true;
                options.IsOptional = false;
                options.MinCount = 1;
            }

            if (!hasOptions)
                return null;

            //make sure to move next...
            c.MoveNext();

            return options;
        }

        private object CreateRangeExpr(RegexParseContext c)
        {
            if (!c.HasPrevAccepted)
            {
                throw new Exception("Invalid pattern range, expected a range terminal before '-'!");
            }
            //Move to the next!
            if (!c.MoveNext())
            {
                throw new Exception("Invalid pattern range, unexpected end of input, expected a range terminal after '-'!");
            }

            return new RegexRangeExpr(c.PrevAccepted, c.AcceptChar());
        }

        private RegexTerminalExpr CreateTerminalExpr(RegexParseContext c)
        {
            return new RegexTerminalExpr(c.AcceptChar());
        }
    }

    class RegexParseContextState
    {
        public RegexParseContextState(TextReader input)
        {
            _input = input;
        }

        public RegexParseContextState(RegexParseContextState fromState)
        {
            Refresh(fromState);
        }

        public bool _isEscaped;
        public bool _isAtEnd;
        public char _char;
        public bool _hasNextChar;
        public char _nextChar;
        public char _prevAccepted;
        public bool _hasPrevAccepted;
        public TextReader _input;
        public bool _prevWasEscaped;

        public void ResumeFromSubContext(RegexParseContextState fromState)
        {
            Refresh(fromState);
        }

        public RegexParseContextState Refresh(RegexParseContextState fromState)
        {
            _isEscaped = fromState._isEscaped;
            _isAtEnd = fromState._isAtEnd;
            _char = fromState._char;
            _hasNextChar = fromState._hasNextChar;
            _nextChar = fromState._nextChar;
            _input = fromState._input;
            _prevWasEscaped = fromState._prevWasEscaped;

            return this;
        }

        public void StartForNewContext()
        {
            _hasPrevAccepted = false;
            _prevAccepted = '\0';
        }
    }

    class RegexParseContext
    {
        public static bool operator ==(RegexParseContext context, char ch)
        {
            if (context.IsAtEnd)
                return false;

            return context.Char == ch;
        }

        public static bool operator !=(RegexParseContext context, char ch)
        {
            if (context.IsAtEnd)
                return false;

            return context.Char != ch;
        }

        public static bool operator ==(char ch, RegexParseContext context)
        {
            if (context.IsAtEnd)
                return false;

            return context.Char == ch;
        }

        public static bool operator !=(char ch, RegexParseContext context)
        {
            if (context.IsAtEnd)
                return false;

            return context.Char != ch;
        }

        private RegexParseContextState _state;

        private RegexParseContextState state
        {
            get
            {
                if (_activeSubContext != null)
                {
                    ResumeFromActiveSubContext();
                }

                return _state;
            }
            set { _state = value; }
        }

        private RegexParseContext _activeSubContext;
        private bool _addExprAsAlteration;
        private bool _addExprAsProduction;
        private RegexPatternExpr _altExpr;
        private RegexPatternExpr _prodExpr;
        private List<RegexPatternExpr> _prodExpressions;

        private bool _isEscaped { get { return state._isEscaped; } set { state._isEscaped = value; } }
        private bool _isAtEnd { get { return state._isAtEnd; } set { state._isAtEnd = value; } }
        private bool _hasNextChar { get { return state._hasNextChar; } set { state._hasNextChar = value; } }
        private char _nextChar { get { return state._nextChar; } set { state._nextChar = value; } }
        private char _char { get { return state._char; } set { state._char = value; } }
        private char _prevAccepted { get { return state._prevAccepted; } set { state._prevAccepted = value; } }
        private bool _hasPrevAccepted { get { return state._hasPrevAccepted; } set { state._hasPrevAccepted = value; } }
        private TextReader _input { get { return state._input; } set { state._input = value; } }
        private bool _prevWasEscaped { get { return state._prevWasEscaped; } set { state._prevWasEscaped = value; } }

        public RegexParseContext(TextReader input)
        {
            state = new RegexParseContextState(input);
            _addExprAsProduction = true;
            _prodExpressions = new List<RegexPatternExpr>();
        }

        public RegexParseContext(RegexParseContextState fromState)
        {
            state = new RegexParseContextState(fromState);
            state.StartForNewContext();
            _addExprAsProduction = true;
            _prodExpressions = new List<RegexPatternExpr>();
        }

        public bool IsAtEnd { get { return _isAtEnd; } set { _isAtEnd = value; } }

        public bool IsEscaped { get { return _isEscaped; } set { _isEscaped = value; } }

        public char Char { get { return _char; } }

        public char NextChar
        {
            get
            {
                if (_hasNextChar)
                {
                    return _nextChar;
                }
                else
                {
                    var i = _input.Peek();
                    if (i == -1)
                    {
                        _hasNextChar = false;
                        _nextChar = '\0';
                    }
                    else
                    {
                        _hasNextChar = true;
                        _nextChar = (char)i;
                    }
                }

                return _nextChar;
            }
        }

        public char AcceptChar()
        {
            _prevAccepted = Char;
            _hasPrevAccepted = true;
            return Char;
        }

        public bool HasPrevAccepted { get { return _hasPrevAccepted; } }

        public char PrevAccepted
        {
            get { return _prevAccepted; }
        }

        public bool MoveNext()
        {
            if (IsAtEnd)
                return false;

            var i = _input.Read();
            if (i == -1)
            {
                _char = '\0';
                IsEscaped = false;
                IsAtEnd = true;
                return false;
            }

            _char = (char)i;
            _hasNextChar = false;
            _nextChar = '\0';

            if (_prevWasEscaped)
            {
                IsEscaped = false;
                _prevWasEscaped = false;
            }

            if (_char == '\\')
            {
                if (!IsEscaped)
                {
                    IsEscaped = true;
                    return MoveNext();
                }
            }

            return true;
        }

        public RegexParseContext New()
        {
            _activeSubContext = new RegexParseContext(state);
            return _activeSubContext;
        }

        private void ResumeFromActiveSubContext()
        {
            if (_activeSubContext != null)
            {
                _state.ResumeFromSubContext(_activeSubContext._state);
                _activeSubContext = null;
            }
        }

        public RegexPatternExpr ToExpression()
        {
            var rootExpr = _altExpr;

            foreach (var prodExpr in _prodExpressions)
            {
                if (rootExpr == null)
                {
                    rootExpr = prodExpr;
                }
                else
                {
                    rootExpr.AddExpr(prodExpr);
                }
            }

            if (rootExpr.CanBeSimplified)
            {
                var rootExprSimplified = rootExpr.GetSimplified();
                if (rootExprSimplified != null)
                {
                    return rootExprSimplified;
                }
            }

            return rootExpr;
        }

        public void AddExpr(RegexPatternExpr expr)
        {
            if (_addExprAsAlteration)
            {
                if(_altExpr == null)
                    _altExpr = RegexPatternExpr.Alteration();

                _prodExpr = RegexPatternExpr.Production();
                _prodExpressions.Add(_prodExpr);
                _prodExpr.AddExpr(expr);

                AddNextAsProduction();
                return;
            }

            if (_prodExpr == null)
            {
                _prodExpr = RegexPatternExpr.Production();
                _prodExpressions.Add(_prodExpr);
            }
            
            _prodExpr.AddExpr(expr);

            AddNextAsProduction();
        }

        public void AddNextAsAlteration()
        {
            _addExprAsAlteration = true;
            _addExprAsProduction = false;
        }

        private void AddNextAsProduction()
        {
            _addExprAsProduction = true;
            _addExprAsAlteration = false;
        }
    }
}
