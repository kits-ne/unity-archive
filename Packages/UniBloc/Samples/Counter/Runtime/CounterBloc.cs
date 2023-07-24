namespace UniBloc.Samples.Counter
{
    public class CounterBloc : PooledBloc<CounterEvent, int>
    {
        public CounterBloc() : base(0)
        {
            On<CounterEvent.Increment>((e, emitter) => { emitter.Emit(State + 1); });
            On<CounterEvent.Decrement>((e, emitter) => { emitter.Emit(State - 1); });
        }
    }

    public class CounterEvent : EventBase<CounterEvent>
    {
        public sealed class Increment : CounterEvent
        {
            public override string ToString() => nameof(Increment);
        }

        public sealed class Decrement : CounterEvent
        {
            public override string ToString() => nameof(Decrement);
        }
    }
}