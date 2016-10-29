using System;
using System.Collections.Generic;
using RapidPliant.Collections;
using RapidPliant.Util;

namespace RapidPliant.Automata
{
    public interface IStateGraph
    {
        IGraphState StartState { get; }

        IReadOnlyList<IGraphState> States { get; }

        void EnsureCompiled();
    }
    
    public abstract class Graph<TState> : IStateGraph, IDisposable
        where TState : IGraphState
    {
        protected bool _hasBuilt;

        protected UniqueList<TState> _states;

        protected int NextAvailableStateId { get; set; }

        protected Graph()
            : this(default(TState))
        {
        }
        
        protected Graph(TState startState)
        {
            StartState = startState;

            _states = ReusableUniqueList<TState>.GetAndClear();

            NextAvailableStateId = 1;
        }

        public TState StartState { get; protected set; }
        IGraphState IStateGraph.StartState { get { return StartState; } }

        public IReadOnlyList<TState> States { get { return _states; } }
        IReadOnlyList<IGraphState> IStateGraph.States { get { return (IReadOnlyList<IGraphState>)States; } }

        public void EnsureCompiled()
        {
            if(_hasBuilt)
                return;

            BuildForRoot();
        }

        protected bool AddState(TState state)
        {
            return _states.AddIfNotExists(state);
        }

        protected void BuildForRoot()
        {
            var startState = GetStartState();
            if(startState == null)
                return;

            BuildForState(startState);

            _hasBuilt = true;
        }

        protected void BuildForState(TState state)
        {
            if(state.Id == 0)
                state.Id = GenerateStateId(state);

            if (!AddState(state))
                return;

            BuildState(state);

            var transitions = GetStateTransitions(state);

            //Build for the transitions
            foreach (var transition in transitions)
            {
                transition.EnsureFromState(state);
                
                var toState = GetTransitionToState(transition);
                if (toState != null)
                {
                    transition.EnsureToState(toState);
                    
                    BuildForState((TState)toState);
                }
            }
        }

        protected virtual void BuildState(TState state)
        {
        }

        protected virtual TState GetStartState()
        {
            return StartState;
        }

        protected virtual int GenerateStateId(TState state)
        {
            return NextAvailableStateId++;
        }
        
        protected virtual IEnumerable<IGraphTransition> GetStateTransitions(IGraphState state)
        {
            return state.Transitions;
        }

        protected virtual IGraphState GetTransitionToState(IGraphTransition transition)
        {
            return transition.ToState;
        }

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
}