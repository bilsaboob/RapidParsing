using System;
using System.Collections.Generic;
using RapidPliant.Collections;

namespace RapidPliant.Automata
{
    public abstract class Graph<TRoot, TState, TTransition, TBuildContext>
        where TRoot : class
        where TState : class, IGraphState
        where TBuildContext : GraphBuildContext<TRoot, TState, TTransition>, new()
    {
        private UniqueList<TState> _states;

        private int NextAvailableStateId { get; set; }

        protected Graph()
            : this(null, null)
        {
        }

        protected Graph(TRoot root)
            : this(root, null)
        {
        }

        protected Graph(TRoot root, TState startState)
        {
            StartState = startState;

            _states = new UniqueList<TState>();
            NextAvailableStateId = 1;

            if (root != null)
            {
                BuildForRoot(root);
            }
        }

        public TRoot Root { get; protected set; }

        public TState StartState { get; protected set; }

        public IReadOnlyList<TState> States { get { return _states; } }

        protected bool AddState(TState state)
        {
            return _states.AddIfNotExists(state);
        }

        protected void BuildForRoot(TRoot root)
        {
            Root = root;
            StartState = GetStartState(root);

            using (var buildContext = CreateBuildContext())
            {
                BuildForState(buildContext, StartState);
            }
        }

        protected virtual TBuildContext CreateBuildContext()
        {
            return new TBuildContext();
        }

        protected void BuildForState(TBuildContext c, TState state)
        {
            if (!state.IsValid)
            {
                state.Id = GenerateStateId(state);
            }
            
            if(!AddState(state))
                return;
            
            BuildState(c, state);

            var transitions = GetStateTransitions(state);

            //Build for the transitions
            foreach (var transition in transitions)
            {
                var toState = GetTransitionToState(transition);
                if (toState != null)
                {
                    c.EnterTransition(transition);
                    BuildForState(c, toState);
                }
            }
        }

        protected virtual void BuildState(TBuildContext c, TState state)
        {
        }

        protected virtual TState GetStartState(TRoot root)
        {
            if (StartState != null)
                return StartState;

            throw new Exception("No start state available!");
        }

        protected virtual int GenerateStateId(TState state)
        {
            return NextAvailableStateId++;
        }

        protected abstract TState GetTransitionToState(TTransition transition);
        protected abstract IEnumerable<TTransition> GetStateTransitions(TState state);
    }

    public abstract class Graph<TRoot, TState, TTransition> : Graph<TRoot, TState, TTransition, GraphBuildContext<TRoot, TState, TTransition>>
        where TRoot : class
        where TState : class, IGraphState
    {
        protected Graph()
            : base(null, null)
        {
        }

        protected Graph(TRoot root)
            : base(root, null)
        {
        }

        protected Graph(TRoot root, TState startState)
            : base(root, startState)
        {
        }
    }

    public class GraphBuildContext<TRoot, TState, TTransition> : IDisposable
        where TRoot : class
        where TState : class, IGraphState
    {
        public GraphBuildContext()
        {
        }

        public virtual void EnterTransition(TTransition transition)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}