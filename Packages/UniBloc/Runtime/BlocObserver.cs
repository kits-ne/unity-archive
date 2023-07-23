using System;

namespace UniBloc
{
    public abstract class BlocObserver
    {
        public virtual void OnCreate(IBloc bloc)
        {
        }

        public virtual void OnEvent(IBloc bloc, object @event)
        {
        }

        public virtual void OnChange(IBloc bloc, IChange change)
        {
        }

        public virtual void OnTransition(IBloc bloc, ITransition transition)
        {
        }

        public virtual void OnError(IBloc bloc, Exception error)
        {
        }

        public virtual void OnDispose(IBloc bloc)
        {
        }
    }
}