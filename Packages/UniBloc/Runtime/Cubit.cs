using System;

namespace UniBloc
{
    public abstract class Cubit<TState> : BlocBase<TState> where TState : IEquatable<TState>
    {
        protected Cubit(TState initialState) : base(initialState)
        {
        }
    }
}