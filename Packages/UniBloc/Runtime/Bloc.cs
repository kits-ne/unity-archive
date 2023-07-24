using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public delegate UniTask EventHandler<in TEvent, out TState>(TEvent @event, IEmitter<TState> emitter);

    public delegate Stream<TEvent> EventMapper<TEvent>(TEvent @event);

    public delegate Stream<TEvent> EventTransformer<TEvent>(Stream<TEvent> events, EventMapper<TEvent> mapper);

    public static class Bloc
    {
        public static BlocObserver Observer = DefaultBlocObserver.Instance;

        // public static readonly EventTransformer<object> Transformer = (events, mapper) =>
        // {
        //     return new Stream<object>();
        // };
        // static EventTransformer<object> transformer = (events, mapper) {
        //     return events
        //         .map(mapper)
        //         .transform<dynamic>(const _FlatMapStreamTransformer<dynamic>());
        // };
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

        // final _eventTransformer =
        //     // ignore: deprecated_member_use_from_same_package
        //     BlocOverrides.current?.eventTransformer ?? Bloc.transformer;

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

        protected void On<T>(Action<T, IEmitter<TState>> handler) where T : TEvent
        {
            ThrowIfExistsHandler<T>();
            _handlers.Add(new Handler(_ => _.GetType() == typeof(T), typeof(T)));

            var subscription = _eventController.Source()
                .Where(e => e is T)
                .Subscribe(@event =>
                {
                    var controller = EmitController<T>.Get(this, @event, handler);
                    EmitHandler<T>.HandleEvent(controller);

                    // var emitter = new Emitter<TState>(OnEmit);
                    // HandleEvent();
                    // var controller = new ChannelController<T>(onDispose: emitter.Cancel);
                    // void HandleEvent()
                    // {
                    //     try
                    //     {
                    //         _emitters.Add(emitter);
                    //         handler((T) @event, emitter);
                    //     }
                    //     catch (Exception e)
                    //     {
                    //         OnErrorInternal(e);
                    //         throw;
                    //     }
                    //     finally
                    //     {
                    //         // OnDone();
                    //         emitter.Complete();
                    //         _emitters.Remove(emitter);
                    //         OnDoneEvent(@event);
                    //     }
                    // }
                    //
                    // void OnEmit(TState state)
                    // {
                    //     if (IsDisposed) return;
                    //     if (State.Equals(state) && Emitted) return;
                    //     OnTransitionInternal(new Transition<TEvent, TState>(
                    //         State,
                    //         @event,
                    //         state
                    //     ));
                    //     Emit(state);
                    // }
                });
            _subscriptions.Add(subscription);
        }

        // EventTransformer<T> transformer = null
        protected void On<T>(EventHandler<T, TState> handler) where T : TEvent
        {
            ThrowIfExistsHandler<T>();
            _handlers.Add(new Handler(_ => _.GetType() == typeof(T), typeof(T)));

            var subscription = _eventController.Source()
                .Where(e => e is T)
                .Subscribe(@event =>
                {
                    var emitController = EmitAsyncController<T>.Get(this, @event, handler);
                    EmitAsyncHandler<T>.HandleEvent(emitController);

                    // var emitter = new Emitter<TState>(OnEmit);
                    // HandleEvent().Forget();
                    // var controller = new ChannelController<T>(onDispose: emitter.Cancel);
                    // async UniTaskVoid HandleEvent()
                    // {
                    //     try
                    //     {
                    //         _emitters.Add(emitter);
                    //         await handler((T) @event, emitter);
                    //     }
                    //     catch (Exception e)
                    //     {
                    //         OnErrorInternal(e);
                    //         throw;
                    //     }
                    //     finally
                    //     {
                    //         // OnDone
                    //         emitter.Complete();
                    //         _emitters.Remove(emitter);
                    //         OnDoneEvent(@event);
                    //     }
                    // }
                    //
                    // void OnEmit(TState state)
                    // {
                    //     if (IsDisposed) return;
                    //     if (State.Equals(state) && Emitted) return;
                    //     OnTransitionInternal(new Transition<TEvent, TState>(
                    //         State,
                    //         @event,
                    //         state
                    //     ));
                    //     Emit(state);
                    // }
                });
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

            await UniTask.WhenAll(_emitters.Select(_ => _.CompleteTask));
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