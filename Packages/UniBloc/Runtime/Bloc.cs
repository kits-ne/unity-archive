using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace UniBloc
{
    public class EventBase<T> : IEquatable<T> where T : class
    {
        public bool Equals(T other) => this == other;
    }

    public interface IBlocEventSink<in TEvent> : IErrorSink
    {
        void Add(TEvent @event);
    }

    public delegate UniTask EventHandler<in TEvent, out TState>(TEvent @event, IEmitter<TState> emitter,
        CancellationToken cancellationToken);

    public static class Bloc
    {
        public static BlocObserver Observer = DefaultBlocObserver.Instance;
    }

    public abstract partial class Bloc<TEvent, TState> : BlocBase<TState>, IBlocEventSink<TEvent>
        where TState : IEquatable<TState>
        where TEvent : class, IEquatable<TEvent>
    {
        protected Bloc(TState initialState) : base(initialState)
        {
        }

        private readonly ChannelController<TEvent> _eventController = new();
        private readonly List<IDisposable> _subscriptions = new();
        private readonly List<Handler> _handlers = new();

        private readonly HashSet<IEmitter> _emitters = new();

        public void Add(TEvent @event)
        {
            ThrowIfNotExistsHandler();
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

            void ThrowIfNotExistsHandler()
            {
                var exists = _handlers.Any(handler => handler.IsType(@event));
                if (!exists)
                {
                    var eventType = @event.GetType();
                    var msg = $"add({eventType}) was called without a registered event handler.\n";
                    msg += $"Make sure to register a handler via on<{eventType}>((event, emit) {{...}})";
                    throw new StateException(msg);
                }
            }
        }

        private void OnEventInternal(TEvent @event)
        {
            OnEvent(@event);
            BlocObserver.OnEvent(this, @event);
        }

        protected virtual void OnEvent(TEvent @event)
        {
        }

        private IUniTaskAsyncEnumerable<TEvent> GetFilteredEventSource<T>() =>
            _eventController
                .Source()
                .Where(e => e is T);


        protected void On<T>(Action<T, IEmitter<TState>> handler) where T : TEvent
        {
            ThrowIfExistsHandler<T>();
            _handlers.Add(new Handler(_ => _.GetType() == typeof(T), typeof(T)));

            var subscription = GetFilteredEventSource<T>()
                .Subscribe(@event =>
                {
                    var controller = EmitController<T>.Get(this, @event, handler);
                    EmitHandler<T>.HandleEvent(controller);
                });
            _subscriptions.Add(subscription);
        }

        protected void On<T>(
            EventHandler<T, TState> handler,
            ConcurrencyMode mode = ConcurrencyMode.Concurrent) where T : TEvent
        {
            ThrowIfExistsHandler<T>();
            _handlers.Add(new Handler(_ => _.GetType() == typeof(T), typeof(T)));

            var source = GetFilteredEventSource<T>();
            var subscription = mode switch
            {
                ConcurrencyMode.Concurrent => source.Subscribe(e =>
                {
                    var emitController = EmitAsyncController<T>.Get(this, e, handler);
                    EmitAsyncHandler<T>.HandleEvent(emitController, DisposeToken);
                }),
                ConcurrencyMode.Sequential => source.Queue().SubscribeAwait(e =>
                {
                    var emitController = EmitAsyncController<T>.Get(this, e, handler);
                    return EmitAsyncHandler<T>.HandleEvent(emitController, DisposeToken);
                }),
                // ConcurrencyMode.Restartable => source.Restart(DisposeToken).SubscribeAwait(tuple =>
                // {
                //     var (e, token) = tuple;
                //     var emitController = EmitAsyncController<T>.Get(this, e, handler);
                //     return EmitAsyncHandler<T>.HandleEvent(emitController, token);
                // }),
                ConcurrencyMode.Droppable => source.SubscribeAwait(e =>
                {
                    var emitController = EmitAsyncController<T>.Get(this, e, handler);
                    return EmitAsyncHandler<T>.HandleEvent(emitController, DisposeToken);
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
            _subscriptions.Add(subscription);
        }

        protected virtual void OnDoneEvent(TEvent @event)
        {
        }

        private void ThrowIfExistsHandler<T>()
        {
            var exists = _handlers.Any(_ => _.Type == typeof(T));
            if (exists)
            {
                var msg = $"on<{typeof(T)}> was called multiple times. \n";
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

            await UniTask.WhenAll(_emitters.Select(emitter => emitter.CompleteTask));
            foreach (var emitter in _emitters)
            {
                if (emitter is IDisposable disposable)
                    disposable.Dispose();
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

    class Handler
    {
        public Func<object, bool> IsType { get; }
        public Type Type { get; }

        public Handler(Func<object, bool> isType, Type type)
        {
            IsType = isType;
            Type = type;
        }
    }

    class DefaultBlocObserver : BlocObserver
    {
        public static readonly DefaultBlocObserver Instance = new();

        private DefaultBlocObserver()
        {
        }
    }
}