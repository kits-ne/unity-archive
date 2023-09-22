using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public interface IStreamable<TState>
    {
        Stream<TState> Stream { get; }
    }

    public interface IStateStreamable<TState> : IStreamable<TState>
    {
        TState State { get; }
    }

    public interface IStateStreamableSource<TState> : IStateStreamable<TState>, IAsyncDisposableWithFlag
    {
    }

    public interface IAsyncDisposableWithFlag : IUniTaskAsyncDisposable
    {
        bool IsDisposed { get; }
    }

    public interface IEmittable<in TState>
    {
        void Emit(TState state);
    }

    public interface IErrorSink : IAsyncDisposableWithFlag
    {
        void AddError(Exception error);
    }

    public interface IBloc
    {
    }

    public abstract class BlocBase<TState>
        : IBloc, IStateStreamableSource<TState>, IEmittable<TState>, IErrorSink
        where TState : IEquatable<TState>
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        protected CancellationToken DisposeToken => _cancellationTokenSource.Token;

        protected BlocBase(TState state = default)
        {
            _state = state;
            Bloc.Observer.OnCreate(this);
        }

        // final _blocObserver = BlocOverrides.current?.blocObserver ?? Bloc.observer;
        protected readonly BlocObserver BlocObserver = Bloc.Observer;

        // 1:n subscribe
        // late final _stateController = StreamController<State>.broadcast();

        // Future -> Task, Single Request
        // Stream -> UniRx, Subscribe

        private readonly ChannelController<TState> _stateController = new();
        protected bool Emitted = false;

        private TState _state;

        public TState State => _state;

        protected virtual void SetState(TState state) => _state = state;

        public Stream<TState> Stream => _stateController.Source().ToStream();
        public bool IsDisposed => _stateController.IsDisposed;

        // protected void Emit(TState state) => (this as IEmittable<TState>).Emit(state);

        void IEmittable<TState>.Emit(TState state)
        {
            try
            {
                if (IsDisposed)
                {
                    throw new StateException("Cannot emit new states after calling close");
                }

                if (State.Equals(state) && Emitted) return;
                OnChangeInternal(new Change<TState>(
                    State, state
                ));
                SetState(state);
                _stateController.Publish(State);
                Emitted = true;
            }
            catch (Exception e)
            {
                OnErrorInternal(e);
                throw;
            }
        }

        private void OnChangeInternal(Change<TState> change)
        {
            OnChange(change);
            Bloc.Observer.OnChange(this, change);
        }

        protected virtual void OnChange(Change<TState> change)
        {
        }

        public virtual void AddError(Exception error)
        {
            OnErrorInternal(error);
        }

        internal void OnErrorInternal(Exception error)
        {
            OnError(error);
            Bloc.Observer.OnError(this, error);
        }

        protected virtual void OnError(Exception error)
        {
        }

        public virtual UniTask DisposeAsync()
        {
            Bloc.Observer.OnDispose(this);
            _stateController.Dispose();
            _cancellationTokenSource.Cancel();
            return UniTask.CompletedTask;
        }
    }
}