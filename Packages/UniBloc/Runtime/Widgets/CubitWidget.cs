using System;

namespace UniBloc.Widgets
{
    public abstract class CubitWidget<TCubit, TState> : BlocWidgetBase<TCubit, TState>
        where TCubit : Cubit<TState>, new()
        where TState : IEquatable<TState>, new()
    {
    }
}