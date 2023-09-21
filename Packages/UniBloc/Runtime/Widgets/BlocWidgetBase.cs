using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UniBloc.Widgets
{
    public abstract class BlocWidgetBase<TBloc, TState> : MonoBehaviour
        where TBloc : BlocBase<TState>, new()
        where TState : IEquatable<TState>

    {
        private TBloc _bloc;

        protected TState State => _bloc.State;
        protected TBloc Bloc => _bloc;
        public Stream<TState> Stream => Bloc.Stream;

        protected virtual void Awake()
        {
            _bloc = CreateBloc();
            Subscribe(Render);
            OnCreated();
        }

        protected virtual TBloc CreateBloc() => new();

        protected abstract void OnCreated();

        protected virtual void OnEnable() => Render(_bloc.State);

        protected void Subscribe(Action<TState> action)
        {
            _bloc.Stream.Subscribe(action).AddTo(destroyCancellationToken);
        }

        protected abstract void Render(TState state);

        protected virtual void OnDestroy()
        {
            _bloc?.DisposeAsync();
        }
    }
}