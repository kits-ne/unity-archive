using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace UniBloc
{
    public interface IEventEntity<out TID> where TID : IEquatable<TID>
    {
        TID ID { get; }
    }

    public abstract partial class ValueBloc<TID, TEvent, TState> : BlocBase<TState>, IBlocEventSink<TEvent>
        where TState : struct, IEquatable<TState>
        where TEvent : struct, IEquatable<TEvent>, IEventEntity<TID>
        where TID : IEquatable<TID>

    {
        protected ValueBloc(TState initialState) : base(initialState)
        {
        }

        private readonly ChannelController<TEvent> _eventController = new();
        private readonly List<IDisposable> _subscriptions = new();
        private readonly HashSet<TID> _handlers = new();

        private readonly HashSet<IEmitter> _emitters = new();

        public void Add(TEvent @event)
        {
            ThrowIfNotExistsHandler(@event.ID);
            try
            {
                OnEventInternal(@event);
                _eventController.Publish(@event);
            }
            catch (Exception e)
            {
                OnErrorInternal(e);
                throw;
            }

            void ThrowIfNotExistsHandler(TID id)
            {
                var exists = _handlers.Contains(id);
                if (!exists)
                {
                    var msg = $"add({id.ToString()}) was called without a registered event handler.\n";
                    msg += $"Make sure to register a handler via on<{id.ToString()}>((event, emit) {{...}})";
                    throw new StateException(msg);
                }
            }
        }

        private void OnEventInternal(TEvent @event)
        {
            OnEvent(@event);
        }

        protected virtual void OnEvent(TEvent @event)
        {
        }

        protected void On(TID id, Action<TEvent, IEmitter<TState>> handler)
        {
            ThrowIfExistsHandler(id);
            _handlers.Add(id);

            var subscription = GetFilteredEventSource(id)
                .Subscribe(e =>
                {
                    var controller = EmitController.Get(this, e, handler);
                    EmitHandler.HandleEvent(controller);
                });
            _subscriptions.Add(subscription);
        }

        protected void On(TID id, EventHandler<TEvent, TState> handler)
        {
            ThrowIfExistsHandler(id);
            _handlers.Add(id);

            var subscription = GetFilteredEventSource(id)
                .Subscribe(e =>
                {
                    var emitController = EmitAsyncController.Get(this, e, handler);
                    EmitAsyncHandler.HandleEvent(emitController);
                });
            _subscriptions.Add(subscription);
        }

        private IUniTaskAsyncEnumerable<TEvent> GetFilteredEventSource(TID id) =>
            _eventController
                .Source()
                .Where(e => e.ID.Equals(id));

        protected virtual void OnDoneEvent(TEvent @event)
        {
        }

        private void ThrowIfExistsHandler(TID id)
        {
            var exists = _handlers.Contains(id);
            if (exists)
            {
                var msg = $"on<{id.ToString()}> was called multiple times. \n";
                msg += "There should only be a single event handler per event type.";
                throw new StateException(msg);
            }
        }

        private void OnTransitionInternal(Transition<TEvent, TState> transition)
        {
            OnTransition(transition);
            Bloc.Observer.OnTransition(this, transition);
        }

        protected virtual void OnTransition(Transition<TEvent, TState> transition)
        {
        }

        public override async UniTask DisposeAsync()
        {
            _eventController.Dispose();

            await UniTask.WhenAll(Enumerable.Select(_emitters, _ => _.CompleteTask));
            foreach (var emitter in _emitters)
            {
                emitter.Dispose();
            }

            _emitters.Clear();
            _handlers.Clear();
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();

            await base.DisposeAsync();
        }
    }
}