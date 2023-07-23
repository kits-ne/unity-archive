using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace UniBloc
{
    public interface ITransition
    {
    }

    public class Transition<TEvent, TState> : ITransition, IEquatable<Transition<TEvent, TState>>
        where TState : IEquatable<TState>
        where TEvent : IEquatable<TEvent>
    {
        public readonly TState CurrentState;
        public readonly TEvent Event;
        public readonly TState NextState;

        public Transition(TState currentState, TEvent @event, TState nextState)
        {
            CurrentState = currentState;
            Event = @event;
            NextState = nextState;
        }

        public bool Equals(Transition<TEvent, TState> other)
        {
            Assert.IsNotNull(other);
            return CurrentState.Equals(other.CurrentState) &&
                   Event.Equals(other.Event) &&
                   NextState.Equals(other.NextState);
        }

        public override int GetHashCode()
        {
            var stateComparer = EqualityComparer<TState>.Default;
            var current = stateComparer.GetHashCode(CurrentState);
            var evt = EqualityComparer<TEvent>.Default.GetHashCode(Event);
            var next = stateComparer.GetHashCode(NextState);
            return current ^ evt ^ next;
        }

        public override string ToString()
        {
            return
                $"Transition {{ {nameof(CurrentState)}: {CurrentState}, {nameof(Event)}: {Event}, {nameof(NextState)}: {NextState} }}";
        }
    }
}