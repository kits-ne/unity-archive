using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine.UI;

namespace UniBloc.Widgets
{
    public abstract class PooledBlocWidget<TBloc, TEvent, TState> : BlocWidgetBase<TBloc, TState>
        where TBloc : PooledBloc<TEvent, TState>
        where TEvent : class, IEquatable<TEvent>, new()
        where TState : IEquatable<TState>, new()
    {
        protected void Add<T>(Action<T> modifier = null) where T : class, TEvent, new()
        {
            Bloc.Add(modifier);
        }

        protected void OnClick<T>(Button button) where T : class, TEvent, new()
        {
            button.OnClickAsAsyncEnumerable().Subscribe(_ => Add<T>()).AddTo(destroyCancellationToken);
        }

        protected void OnClick(Button button, Action<AsyncUnit> action)
        {
            button.OnClickAsAsyncEnumerable().Subscribe(action).AddTo(destroyCancellationToken);
        }
    }
}