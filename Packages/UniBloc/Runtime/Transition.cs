using System;

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
            return CurrentState.Equals(other.CurrentState) &&
                   Event.Equals(other.Event) &&
                   NextState.Equals(other.NextState);
        }

        public override int GetHashCode()
        {
            return CurrentState.GetHashCode() ^
                   Event.GetHashCode() ^
                   NextState.GetHashCode();
        }

        public override string ToString()
        {
            return
                $"Transition {{ {nameof(CurrentState)}: {CurrentState}, {nameof(Event)}: {Event}, {nameof(NextState)}: {NextState} }}";
        }
    }
}