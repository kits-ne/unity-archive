using UniBloc;
using UnityEngine;

namespace Samples.Timer.Runtime
{
    public class TimerBloc : PooledBloc<TimerEvent, TimerState>
    {
        private const int InitialDuration = 60;
        private float _seconds;

        public TimerBloc() : base(new TimerInitial() {Duration = InitialDuration})
        {
            UsingStatePool();

            On<TimerStarted>(OnStarted);
            On<TimerPaused>(OnPaused);
            On<TimerResumed>(OnResumed);
            On<TimerReset>(OnReset);

            On<TimerTicked>(OnTicked);
        }


        protected override void OnTransition(Transition<TimerEvent, TimerState> transition)
        {
            Debug.Log(transition);
        }

        private void OnStarted(TimerStarted started, IEmitter<TimerState> emitter)
        {
            var state = GetState<TimerRunInProgress>();
            state.Duration = started.Duration;
            emitter.Emit(state);
        }

        private void OnPaused(TimerPaused paused, IEmitter<TimerState> emitter)
        {
            if (State is TimerRunInProgress)
            {
                var state = GetState<TimerRunPause>();
                state.Duration = State.Duration;
                emitter.Emit(state);
            }
        }

        private void OnResumed(TimerResumed resumed, IEmitter<TimerState> emitter)
        {
            if (State is TimerRunPause)
            {
                var state = GetState<TimerRunInProgress>();
                state.Duration = State.Duration;
                emitter.Emit(state);
            }
        }

        private void OnReset(TimerReset reset, IEmitter<TimerState> emitter)
        {
            var state = GetState<TimerInitial>();
            state.Duration = InitialDuration;
            emitter.Emit(state);
        }

        private void OnTicked(TimerTicked ticked, IEmitter<TimerState> emitter)
        {
            TimerState state;
            if (ticked.Duration > 0)
            {
                var runInState = GetState<TimerRunInProgress>();
                runInState.Duration = ticked.Duration;
                state = runInState;
            }
            else
            {
                state = GetState<TimerRunComplete>();
            }

            emitter.Emit(state);
        }

        public void Tick(float deltaTime)
        {
            _seconds += deltaTime;

            if (_seconds < 1) return;

            _seconds = 0;
            Add<TimerTicked>(ticked => ticked.Duration = State.Duration - 1);
        }

        sealed class TimerTicked : TimerEvent
        {
            public int Duration;
            public override string ToString() => nameof(TimerTicked);
        }
    }

    public record TimerState
    {
        public virtual int Duration { get; set; }
    }

    public sealed record TimerInitial : TimerState
    {
        public override string ToString() => $"{nameof(TimerInitial)} {{ duration: {Duration} }}";
    }

    public sealed record TimerRunPause : TimerState
    {
        public override string ToString() => $"{nameof(TimerRunPause)} {{ duration: {Duration} }}";
    }

    public sealed record TimerRunInProgress : TimerState
    {
        public override string ToString() => $"{nameof(TimerRunInProgress)} {{ duration: {Duration} }}";
    }

    public sealed record TimerRunComplete : TimerState
    {
        public override int Duration => 0;

        public override string ToString() => $"{nameof(TimerRunComplete)}";
    }

    public class TimerEvent : EventBase<TimerEvent>
    {
    }

    public sealed class TimerStarted : TimerEvent
    {
        public int Duration;
        public override string ToString() => nameof(TimerStarted);
    }

    public sealed class TimerPaused : TimerEvent
    {
        public override string ToString() => nameof(TimerPaused);
    }

    public sealed class TimerResumed : TimerEvent
    {
        public override string ToString() => nameof(TimerResumed);
    }

    public sealed class TimerReset : TimerEvent
    {
        public override string ToString() => nameof(TimerReset);
    }
}