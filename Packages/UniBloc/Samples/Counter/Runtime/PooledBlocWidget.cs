using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UniBloc.Samples.Counter
{
    public abstract class PooledBlocWidget<TBloc, TEvent, TState> : MonoBehaviour
        where TBloc : PooledBloc<TEvent, TState>, new()
        where TEvent : class, IEquatable<TEvent>, new()
        where TState : IEquatable<TState>, new()
    {
        private TBloc _bloc;
        private CancellationToken _destroyToken;

        protected virtual void Awake()
        {
            _destroyToken = this.GetCancellationTokenOnDestroy();
            _bloc = new TBloc();
            Subscribe(Render);
            OnCreated();
        }

        protected abstract void OnCreated();

        private void OnEnable() => Render(_bloc.State);

        private void Subscribe(Action<TState> action)
        {
            _bloc.Stream.Subscribe(action).AddTo(_destroyToken);
        }

        protected void Add<T>(Action<T> modifier = null) where T : class, TEvent, new()
        {
            _bloc.Add(modifier);
        }

        protected abstract void Render(TState state);

        private void OnDestroy()
        {
            _bloc?.DisposeAsync();
        }

        protected void OnClick<T>(Button button) where T : class, TEvent, new()
        {
            button.OnClickAsAsyncEnumerable().Subscribe(_ => Add<T>()).AddTo(_destroyToken);
        }
    }
}