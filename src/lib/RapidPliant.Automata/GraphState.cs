using System;
using System.Collections.Generic;

namespace RapidPliant.Automata
{
    public interface IGraphState : IDisposable
    {
        int Id { get; set; }
        
        IReadOnlyList<IGraphTransition> Transitions { get; }
    }

    public interface IGraphTransition
    {
        IGraphState FromState { get; }
        IGraphState ToState { get; }

        void EnsureFromState(IGraphState fromState);
        void EnsureToState(IGraphState toState);
    }

    public abstract class GraphStateBase : IGraphState, IComparable<GraphStateBase>, IComparable, IDisposable
    {
        private int _guidHashCode;
        protected int _hashCode;
        private int _id;

        public GraphStateBase()
        {
        }

        protected int GuidHashCode
        {
            get
            {
                if (_guidHashCode == 0)
                    _guidHashCode = Guid.NewGuid().GetHashCode();
                return _guidHashCode;
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                var changed = _id != value;
                _id = value;

                if (_id == 0 && !changed)
                {
                    _hashCode = GuidHashCode;
                }
                else if (changed)
                {
                    _hashCode = _id.GetHashCode();
                }
            }
        }

        IReadOnlyList<IGraphTransition> IGraphState.Transitions { get { return null; } }

        public override int GetHashCode()
        {
            return _hashCode;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj == this)
                return true;

            var graphState = obj as GraphStateBase;
            if (graphState == null)
                return false;

            return Id == graphState.Id;
        }

        public virtual int CompareTo(object obj)
        {
            if (obj == null)
                throw new NullReferenceException();

            var state = obj as GraphStateBase;
            if (state == null)
                throw new ArgumentException("Parameter must be a GraphStateBase", nameof(obj));

            return CompareTo(state);
        }

        public int CompareTo(GraphStateBase obj)
        {
            return GetHashCode().CompareTo(obj.GetHashCode());
        }

        public override string ToString()
        {
            return $"{Id}";
        }

        #region Dispose
        protected bool IsDisposed { get; set; }

        ~GraphStateBase()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(false);
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }
        #endregion
    }

    public abstract class GraphStateBase<TState> : GraphStateBase, IComparable<TState>
        where TState : GraphStateBase<TState>
    {
        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj == this)
                return true;

            var graphState = obj as TState;
            if (graphState == null)
                return false;

            return Id == graphState.Id;
        }

        public override int CompareTo(object obj)
        {
            if (obj == null)
                throw new NullReferenceException();

            var state = obj as TState;
            if (state == null)
                throw new ArgumentException($"Parameter must be a {typeof(TState).Name}", nameof(obj));

            return CompareTo(state);
        }

        public int CompareTo(TState obj)
        {
            return GetHashCode().CompareTo(obj.GetHashCode());
        }
    }
}
