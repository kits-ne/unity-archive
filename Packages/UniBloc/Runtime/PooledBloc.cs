using System;
using System.Collections.Generic;
using System.Linq;

namespace UniBloc
{
    public abstract class PooledBloc<TEvent, TState> : Bloc<TEvent, TState>
        where TEvent : class, IEquatable<TEvent>, new()
        where TState : IEquatable<TState>, new()
    {
        private static readonly Dictionary<Type, Queue<TEvent>> EventPool = new();
        private static Queue<TState> _statePool;

        protected PooledBloc(TState initialState) : base(initialState)
        {
        }

        protected void UsingStatePool()
        {
            _statePool = new();
        }

        public void Add<T>(Action<T> modifier = null) where T : class, TEvent, new()
        {
            if (!EventPool.TryGetValue(typeof(T), out var pool))
            {
                pool = new();
                EventPool.Add(typeof(T), pool);
            }

            var e = pool.Any() ? pool.Dequeue() : new T();
            modifier?.Invoke(e as T);
            Add(e);
        }

        protected override void OnDoneEvent(TEvent @event)
        {
            if (EventPool.TryGetValue(@event.GetType(), out var pool))
            {
                pool.Enqueue(@event);
            }
        }

        protected TState GetState()
        {
            if (_statePool == null) throw new Exception("not using state pool");
            return _statePool.Any() ? _statePool.Dequeue() : new();
        }

        protected sealed override void SetState(TState state)
        {
            if (_statePool == null)
            {
                base.SetState(state);
                return;
            }

            var prevState = State;
            base.SetState(state);
            _statePool.Enqueue(prevState);
        }
    }
}