using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace UniBloc
{
    public interface IChange
    {
    }

    public class Change<TState> : IChange, IEquatable<Change<TState>>
        where TState : IEquatable<TState>
    {
        public readonly TState CurrentState;
        public readonly TState NextState;

        public Change(TState currentState, TState nextState)
        {
            CurrentState = currentState;
            NextState = nextState;
        }

        public bool Equals(Change<TState> other)
        {
            Assert.IsNotNull(other);
            return CurrentState.Equals(other.CurrentState) &&
                   NextState.Equals(other.NextState);
        }

        public override int GetHashCode()
        {
            var comparer = EqualityComparer<TState>.Default;
            var current = comparer.GetHashCode(CurrentState);
            var next = comparer.GetHashCode(NextState);
            return current ^ next;
        }

        public override string ToString()
        {
            return $"Change {{ {nameof(CurrentState)}: {CurrentState}, {nameof(NextState)}: {NextState} }}";
        }
    }
}