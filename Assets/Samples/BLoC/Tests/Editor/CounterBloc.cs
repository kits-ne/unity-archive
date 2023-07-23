using System;
using UniBloc;
using UnityEngine;

namespace Samples.BLoC.Tests.Editor
{
    public class CounterEvent : EventBase<CounterEvent>
    {
    }

    public sealed class CounterIncrementEvent : CounterEvent
    {
        public readonly int Amount;

        public CounterIncrementEvent(int amount = 1)
        {
            Amount = amount;
        }

        public override string ToString() => "Increment";
    }

    public class CounterBloc : Bloc<CounterEvent, int>
    {
        public CounterBloc() : base(0)
        {
            On<CounterIncrementEvent>((e, emitter) =>
            {
                AddError(new Exception("increment error"));
                emitter.Emit(State + e.Amount);
            });
        }

        protected override void OnEvent(CounterEvent @event)
        {
            Debug.Log(@event);
        }

        protected override void OnChange(Change<int> change)
        {
            Debug.Log(change);
        }

        protected override void OnTransition(Transition<CounterEvent, int> transition)
        {
            Debug.Log(transition);
        }

        protected override void OnError(Exception error)
        {
            Debug.Log(error);
        }
    }
}