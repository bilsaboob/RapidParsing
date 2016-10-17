using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using RapidPliant.Lexing.Dfa;
using RapidPliant.Lexing.Lexer;
using RapidPliant.Lexing.Pattern;
using RapidPliant.Lexing.Pattern.Regex;

namespace RapidPliant.App.ViewModels
{
    public class LexDfaGraphViewModel : GraphViewModel
    {
        protected LexDfaFactory LexDfaFactory = new LexDfaFactory();
        protected RapidRegex Regex = new RapidRegex();

        private Graph _graph;
        private Dictionary<int, LexGraphNode> _nodes;

        public LexDfaGraphViewModel()
        {
        }

        protected virtual void LoadDataForLexRules(params LexPatternRule[] lexRules)
        {
            var lexDfa = CreateLexDfa(lexRules);
            LoadDataForLexDfa(lexDfa);
        }

        protected virtual void LoadDataForLexPattern(string lexPattern)
        {
            var lexDfa = CreateLexDfa(lexPattern);
            LoadDataForLexDfa(lexDfa);
        }

        protected virtual void LoadDataForLexDfa(LexDfa lexDfa)
        {
            //Iterate the lex def and create a graph!
            _graph = new Graph();
            _nodes = new Dictionary<int, LexGraphNode>();

            var statesCount = lexDfa.StatesCount;
            var states = lexDfa.States;

            for (var i = 0; i < statesCount; ++i)
            {
                var state = states[i];

                var termTransitions = state.GetTermTransitions();
                foreach (var termTransition in termTransitions)
                {
                    CreateTransition(state, termTransition);
                }
            }

            //Configure the nodes!
            foreach (var stateNode in _nodes.Values)
            {
                ConfigureNode(stateNode);
            }

            //Set the graph!
            Graph = _graph;
        }

        #region Graph helpers
        private void CreateTransition(LexDfaState state, LexDfaStateTransition transition)
        {
            var fromState = state;
            var fromStateNode = GetOrCreateStateNode(fromState);
            if (fromStateNode == null)
                return;

            var toState = transition.ToState;
            if(toState == null)
                return;

            var toStateNode = GetOrCreateStateNode(toState);
            if (toStateNode == null)
                return;

            GetOrCreateTransitionEdge(transition.TransitionSymbol, fromStateNode, toStateNode);
        }

        private LexGraphNode GetStateNode(int id)
        {
            LexGraphNode stateNode;
            _nodes.TryGetValue(id, out stateNode);
            return stateNode;
        }

        private LexGraphNode GetOrCreateStateNode(LexDfaState state)
        {
            LexGraphNode stateNode;
            var stateId = state.StateId;
            if (!_nodes.TryGetValue(stateId, out stateNode))
            {
                stateNode = new LexGraphNode(null, state);
                _nodes[stateId] = stateNode;
            }
            return stateNode;
        }

        private void GetOrCreateTransitionEdge(ILexPatternSymbol symbol, LexGraphNode fromStateNode, LexGraphNode toStateNode)
        {
            var edge = _graph.AddEdge(fromStateNode.State.StateId.ToString(), toStateNode.State.StateId.ToString());
            var transEdge = new TransitionEdge(fromStateNode, toStateNode, edge);
            fromStateNode.OutEdges.Add(transEdge);
            toStateNode.InEdges.Add(transEdge);

            edge.LabelText = GetTransitionLabel(symbol);
        }

        private string GetTransitionLabel(ILexPatternSymbol symbol)
        {
            var charSymbol = symbol as ILexPatternTerminalCharSymbol;
            if (charSymbol != null)
            {
                return charSymbol.Char.ToString();
            }

            var charRangeSymbol = symbol as ILexPatternTerminalRangeSymbol;
            if (charRangeSymbol != null)
            {
                return string.Format("{0}-{1}", charRangeSymbol.FromChar, charRangeSymbol.ToChar);
            }

            if (symbol == null)
            {
                return "<NULL>";
            }

            return "???";
        }

        public void ConfigureNode(LexGraphNode stateNode)
        {
            var n = _graph.FindNode(stateNode.State.StateId.ToString());
            stateNode.Node = n;

            n.Attr.Shape = Shape.Circle;
            n.Attr.XRadius = 1;
            n.Attr.YRadius = 1;
            n.Attr.LineWidth = 1;
            n.UserData = "User data?";

            var state = stateNode.State;
            var transitions = state.GetTermTransitions();
            var isFinal = transitions == null || transitions.Length == 0;
            if (isFinal)
            {
                n.Attr.Shape = Shape.DoubleCircle;
            }
        }
        #endregion

        #region Helpers
        protected LexDfaTableLexer CreateLexer(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern);
            var dfa = CreateLexDfa(patternExpr, name);
            return new LexDfaTableLexer(dfa);
        }

        protected LexDfaTableLexer CreateLexer(RegexPatternExpr patternExpr, string name = null)
        {
            var dfa = CreateLexDfa(patternExpr, name);
            return new LexDfaTableLexer(dfa);
        }

        protected RegexPatternExpr CreateLexExpr(string regexPattern)
        {
            return Regex.FromPattern(regexPattern);
        }

        protected LexDfa CreateLexDfa(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern);
            return CreateLexDfa(patternExpr, name);
        }

        protected LexPatternRule CreateLexRule(string regexPattern, string name = null)
        {
            var patternExpr = CreateLexExpr(regexPattern);
            //Build a rule from the pattern expression
            var patternRule = new LexPatternRule(name);
            patternRule.FromExpression(patternExpr);
            return patternRule;
        }

        protected LexDfa CreateLexDfa(params LexPatternRule[] lexRules)
        {
            //Build dfa for the rule
            var dfa = LexDfaFactory.BuildFromLexPatternRules(lexRules);
            return dfa;
        }

        protected LexDfa CreateLexDfa(RegexPatternExpr patternExpr, string name = null)
        {
            //Build a rule from the pattern expression
            var patternRule = new LexPatternRule(name);
            patternRule.FromExpression(patternExpr);

            //Build dfa for the rule
            var dfa = LexDfaFactory.BuildFromLexPatternRule(patternRule);
            return dfa;
        }
        #endregion
    }

    public class LexGraphNode
    {
        public LexGraphNode(Node node, LexDfaState frame)
        {
            Node = node;
            State = frame;

            OutEdges = new List<TransitionEdge>();
            InEdges = new List<TransitionEdge>();
        }

        public Node Node { get; set; }
        public LexDfaState State { get; set; }

        public List<TransitionEdge> OutEdges { get; set; }
        public List<TransitionEdge> InEdges { get; set; }
    }

    public class TransitionEdge
    {
        public TransitionEdge(LexGraphNode fromState, LexGraphNode toState, Edge edge)
        {
            FromNode = fromState;
            ToNode = toState;
            Edge = edge;
        }

        public LexGraphNode ToNode { get; set; }
        public LexGraphNode FromNode { get; set; }
        public Edge Edge { get; set; }
    }
}
