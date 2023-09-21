using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UniBloc.Widgets
{
    public abstract class BlocWidget<TBloc, TEvent, TState> : BlocWidgetBase<TBloc, TState>
        where TBloc : Bloc<TEvent, TState>, new()
        where TEvent : class, IEquatable<TEvent>
        where TState : IEquatable<TState>
    {
    }
}