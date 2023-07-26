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
        private static Dictionary<Type, Queue<TState>> _statePools;

        protected PooledBloc(TState initialState) : base(initialState)
        {
        }

        protected void UsingStatePool()
        {
            _statePools = new();
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

        protected TState GetState() => GetState<TState>();

        private Queue<TState> GetStatePool(Type stateType)
        {
            if (_statePools == null) throw new Exception("not using state pool");

            if (!_statePools.TryGetValue(stateType, out var pool))
            {
                pool = new Queue<TState>();
                _statePools[stateType] = pool;
            }

            return pool;
        }

        protected T GetState<T>() where T :TState, new()
        {
            if (_statePools == null) throw new Exception("not using state pool");
            var pool = GetStatePool(typeof(T));
            return pool.Any() ? (T) pool.Dequeue() : new T();
        }

        protected sealed override void SetState(TState state)
        {
            if (_statePools == null)
            {
                base.SetState(state);
                return;
            }

            var prevState = State;
            base.SetState(state);
            
            var pool = GetStatePool(prevState.GetType());
            pool.Enqueue(prevState);
        }
    }
}