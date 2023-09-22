using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public abstract partial class ValueBloc<TID, TEvent, TState>
    {
        abstract class EmitControllerBase : IDisposable
        {
            protected ValueBloc<TID, TEvent, TState> Bloc { get; set; }
            protected TEvent Event { get; set; }

            private IEmittable<TState> Emittable => Bloc;

            public void OnEmit(TState state)
            {
                if (Bloc.IsDisposed) return;
                if (Bloc.State.Equals(state) && Bloc.Emitted) return;
                Bloc.OnTransitionInternal(new Transition<TEvent, TState>(
                    Bloc.State,
                    Event,
                    state
                ));
                Emittable.Emit(state);
            }

            public void CompleteEvent(Emitter<TState> emitter)
            {
                emitter.Complete();
                Bloc._emitters.Remove(emitter);
                Bloc.OnDoneEvent(Event);
            }

            public void OnError(Exception e)
            {
                Bloc.OnErrorInternal(e);
            }

            public virtual void Dispose()
            {
                Bloc = null;
                Event = default;
            }
        }

        sealed class EmitController : EmitControllerBase
        {
            public Action<TEvent, IEmitter<TState>> Handler { get; private set; }

            public void HandleEvent(Emitter<TState> emitter)
            {
                Bloc._emitters.Add(emitter);
                Handler(Event, emitter);
            }

            public override void Dispose()
            {
                Handler = null;
                base.Dispose();
                Pool.Enqueue(this);
            }

            private static readonly Queue<EmitController> Pool = new();

            public static EmitController Get(
                ValueBloc<TID, TEvent, TState> bloc,
                TEvent @event,
                Action<TEvent, IEmitter<TState>> handler
            )
            {
                var context = Pool.Any() ? Pool.Dequeue() : new();
                context.Bloc = bloc;
                context.Event = @event;
                context.Handler = handler;
                return context;
            }
        }

        sealed class EmitAsyncController : EmitControllerBase
        {
            public EventHandler<TEvent, TState> Handler { get; private set; }

            public UniTask HandleEventAsync(Emitter<TState> emitter, CancellationToken cancellationToken)
            {
                Bloc._emitters.Add(emitter);
                return Handler(Event, emitter, cancellationToken);
            }

            public override void Dispose()
            {
                Handler = null;
                base.Dispose();
                Pool.Enqueue(this);
            }

            private static readonly Queue<EmitAsyncController> Pool = new();

            public static EmitAsyncController Get(
                ValueBloc<TID, TEvent, TState> bloc,
                TEvent @event,
                EventHandler<TEvent, TState> handler
            )
            {
                var context = Pool.Any() ? Pool.Dequeue() : new();
                context.Bloc = bloc;
                context.Event = @event;
                context.Handler = handler;
                return context;
            }
        }
    }
}