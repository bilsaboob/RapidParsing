using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using RapidPliant.App.ViewModels;

namespace RapidPliant.App.ViewModels
{
    public abstract class MsaglGraphModel<TState, TTransition, TNode, TEdge>
        where TNode : MsaglGraphNode<TState, TTransition, TNode, TEdge>, new()
        where TEdge : MsaglGraphNodeEdge<TState, TTransition, TNode, TEdge>, new()
    {
        protected Graph _graph;
        protected Dictionary<int, TNode> _graphNodes;

        public MsaglGraphModel()
        {
        }

        public Graph Graph { get { return _graph; } }

        public void Build(IEnumerable<TState> states)
        {
            _graph = CreateGraph();
            _graphNodes = new Dictionary<int, TNode>();

            CreateTransitions(states);

            ConfigureNodes();
        }

        protected virtual Graph CreateGraph()
        {
            return new Graph();
        }

        private void ConfigureNodes()
        {
            foreach (var graphNode in _graphNodes.Values)
            {
                ConfigureNode(graphNode);
            }
        }

        private void ConfigureNode(TNode graphNode)
        {
            var node = _graph.FindNode(graphNode.NodeId);
            graphNode.Node = node;

            node.Attr.Shape = Shape.Circle;
            node.Attr.XRadius = 1;
            node.Attr.YRadius = 1;
            node.Attr.LineWidth = 1;

            var state = graphNode.State;
            var isFinal = IsFinalState(state);
            if (isFinal)
            {
                node.Attr.Shape = Shape.DoubleCircle;
            }

            node.LabelText = GetStateLabel(state);

            PopulateGraphNode(graphNode);
        }

        protected virtual string GetStateLabel(TState state)
        {
            return state.ToString();
        }

        protected virtual bool IsFinalState(TState state)
        {
            var transitions = GetStateTransitions(state);
            var isFinal = !transitions.Any();
            return isFinal;
        }

        protected virtual void PopulateGraphNode(TNode graphNode)
        {
        }

        private void CreateTransitions(IEnumerable<TState> states)
        {
            foreach (var fromState in states)
            {
                var transitions = GetStateTransitions(fromState);
                foreach (var transition in transitions)
                {
                    CreateGraphEdge(fromState, transition);
                }
            }
        }

        protected virtual void CreateGraphEdge(TState fromState, TTransition transition)
        {
            CreateTransitionGraphEdge(fromState, transition);
        }

        protected abstract IEnumerable<TTransition> GetStateTransitions(TState state);

        protected TNode GetOrCreateGraphNode(TState state)
        {
            TNode stateNode;
            var stateId = GetStateId(state);
            if (!_graphNodes.TryGetValue(stateId, out stateNode))
            {
                stateNode = CreateGraphNode(state);
                stateNode.State = state;
                stateNode.StateId = stateId;

                _graphNodes[stateId] = stateNode;
            }
            return stateNode;
        }

        protected virtual TNode CreateGraphNode(TState state)
        {
            return new TNode();
        }

        protected abstract int GetStateId(TState state);

        protected void CreateTransitionGraphEdge(TState fromState, TTransition transition)
        {
            var fromGraphNode = GetOrCreateGraphNode(fromState);
            if (fromGraphNode == null)
                return;

            var toState = GetTransitionToState(transition);
            if(toState == null)
                return;

            var toGraphNode = GetOrCreateGraphNode(toState);
            if(toGraphNode == null)
                return;

            var graphEdge = CreateTransitionGraphEdge(fromGraphNode, toGraphNode, transition);
            
            graphEdge.Edge.LabelText = GetTransitionLabel(graphEdge.Transition);

            PopulateGraphEdge(graphEdge);
        }

        protected virtual void PopulateGraphEdge(TEdge graphEdge)
        {
        }

        protected TEdge CreateTransitionGraphEdge(TNode fromNode, TNode toNode, TTransition transition)
        {
            var edge = _graph.AddEdge(fromNode.NodeId, toNode.NodeId);
            var transEdge = CreateGraphEdge(fromNode, toNode, edge);
            transEdge.Transition = transition;
            fromNode.AddOutEdge(transEdge);
            toNode.AddInEdge(transEdge);
            return transEdge;
        }

        protected virtual TEdge CreateGraphEdge(TNode fromNode, TNode toNode, Edge edge)
        {
            var e = new TEdge();
            e.FromNode = fromNode;
            e.ToNode = toNode;
            e.Edge = edge;
            return e;
        }

        protected abstract TState GetTransitionToState(TTransition transition);

        protected virtual string GetTransitionLabel(TTransition trans)
        {
            if (trans == null)
                return null;

            return trans.ToString();
        }
    }

    public class MsaglGraphNode<TState, TTransition, TNode, TEdge>
        where TEdge: MsaglGraphNodeEdge<TState, TTransition, TNode, TEdge>
        where TNode: MsaglGraphNode<TState, TTransition, TNode, TEdge>
    {
        private List<TEdge> _outEdges;
        private List<TEdge> _inEdges;

        public MsaglGraphNode()
        {
            _outEdges = new List<TEdge>();
            _inEdges = new List<TEdge>();
        }

        public object StateId { get; set; }
        public TState State { get; set; }

        public string NodeId { get { return StateId.ToString(); } }
        public Node Node { get; set; }        

        public IReadOnlyList<TEdge> OutEdges { get { return _outEdges; } }
        public IReadOnlyList<TEdge> InEdges { get { return _inEdges; } }

        public void AddInEdge(TEdge edge)
        {
            if(!_inEdges.Contains(edge))
                _inEdges.Add(edge);
        }

        public void AddOutEdge(TEdge edge)
        {
            if (!_outEdges.Contains(edge))
                _outEdges.Add(edge);
        }
    }
    
    public class MsaglGraphNodeEdge<TState, TTransition, TNode, TEdge>
        where TEdge : MsaglGraphNodeEdge<TState, TTransition, TNode, TEdge>
        where TNode : MsaglGraphNode<TState, TTransition, TNode, TEdge>
    {
        public MsaglGraphNodeEdge()
        {
        }

        public TTransition Transition { get; set; }
        public TNode FromNode { get; set; }
        public TNode ToNode { get; set; }
        public Edge Edge { get; set; }
    }

    public abstract class MsaglGraphModel<TState, TTransition> : MsaglGraphModel<TState, TTransition, MsaglGraphNode<TState, TTransition>, MsaglGraphNodeEdge<TState, TTransition>>
    {
    }

    public class MsaglGraphNode<TState, TTransition> : MsaglGraphNode<TState, TTransition, MsaglGraphNode<TState, TTransition>, MsaglGraphNodeEdge<TState, TTransition>>
    {
    }

    public class MsaglGraphNodeEdge<TState, TTransition> : MsaglGraphNodeEdge<TState, TTransition, MsaglGraphNode<TState, TTransition>, MsaglGraphNodeEdge<TState, TTransition>>
    {
    }
}
