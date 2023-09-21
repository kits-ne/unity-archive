using System;
using System.Threading;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine.UI;

namespace UniBloc.Widgets
{
    public abstract class ValueBlocWidget<TBloc, TID, TEvent, TState> : BlocWidgetBase<TBloc, TState>
        where TBloc : ValueBloc<TID, TEvent, TState>, new()
        where TID : IEquatable<TID>
        where TEvent : struct, IEquatable<TEvent>, IEventEntity<TID>
        where TState : struct, IEquatable<TState>
    {
        protected void Add(TEvent @event) => Bloc.Add(@event);
    }
}