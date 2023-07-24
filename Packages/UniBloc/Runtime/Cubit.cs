using System;

namespace UniBloc
{
    public abstract class Cubit<TState> : BlocBase<TState> where TState : IEquatable<TState>
    {
        protected Cubit(TState initialState) : base(initialState)
        {
        }

        private IEmittable<TState> Emittable => this;

        protected void Emit(TState state) => Emittable.Emit(state);
    }
}