using System;
using System.Collections.Generic;
using RapidPliant.Collections;
using RapidPliant.Util;

namespace RapidPliant.Automata
{
    public interface IStateGraph
    {
        void EnsureCompiled();
    }

    public interface IStateGraph<TState> : IStateGraph
    {
        TState StartState { get; }
        IReadOnlyList<TState> States { get; }
    }

    public interface IStateGraph<TRoot, TState> : IStateGraph<TState>
    {
        TRoot Root { get; }
    }

    public abstract class Graph<TRoot, TState, TTransition, TBuildContext> : IStateGraph<TRoot, TState>, IDisposable
        where TRoot : class
        where TState : class, IGraphState
        where TTransition : IGraphTransition
        where TBuildContext : GraphBuildContext<TRoot, TState, TTransition>, new()
    {
        private bool _hasBuilt;

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

            _states = ReusableUniqueList<TState>.GetAndClear();

            NextAvailableStateId = 1;

            if (root != null)
            {
                BuildForRoot(root);
            }
        }

        public TRoot Root { get; protected set; }

        public TState StartState { get; protected set; }

        public IReadOnlyList<TState> States { get { return _states; } }

        public void EnsureCompiled()
        {
            if(_hasBuilt)
                return;

            BuildForRoot(Root);
        }

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

            _hasBuilt = true;
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
                transition.EnsureFromState(state);

                var toState = GetTransitionToState(transition);
                if (toState != null)
                {
                    transition.EnsureToState(toState);

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

        public virtual void Dispose()
        {
            if (_states != null)
            {
                foreach (var state in _states)
                {
                    try
                    {
                        state.Dispose();
                    }
                    catch (Exception)
                    {
                        //Doesn't matter
                    }
                }

                _states.ClearAndFree();
            }
        }   
    }

    public abstract class Graph<TRoot, TState, TTransition> : Graph<TRoot, TState, TTransition, GraphBuildContext<TRoot, TState, TTransition>>
        where TRoot : class
        where TState : class, IGraphState
        where TTransition : IGraphTransition
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