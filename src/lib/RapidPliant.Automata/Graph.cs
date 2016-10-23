using System;
using System.Collections.Generic;
using RapidPliant.Collections;

namespace RapidPliant.Automata
{
    public abstract class Graph<TRoot, TState, TTransition>
        where TRoot : class
        where TState : class, IGraphState
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
            BuildForState(StartState);
        }

        protected void BuildForState(TState state)
        {
            if (!state.IsValid)
            {
                state.Id = GenerateStateId(state);
            }
            
            if(!AddState(state))
                return;

            var transitions = GetStateTransitions(state);

            //Build for the transitions
            foreach (var transition in transitions)
            {
                var toState = GetTransitionToState(transition);
                if (toState != null)
                {
                    BuildForState(toState);
                }
            }
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
}