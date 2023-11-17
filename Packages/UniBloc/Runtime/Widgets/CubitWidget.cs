using System;

namespace UniBloc.Widgets
{
    public abstract class CubitWidget<TCubit, TState> : BlocWidgetBase<TCubit, TState>
        where TCubit : Cubit<TState>
        where TState : IEquatable<TState>, new()
    {
        protected TCubit Cubit => Bloc;
    }
}