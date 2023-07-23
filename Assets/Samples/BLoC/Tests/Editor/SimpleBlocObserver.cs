using System;
using UniBloc;
using UnityEngine;

namespace Samples.BLoC.Tests.Editor
{
    public class SimpleBlocObserver : BlocObserver
    {
        public override void OnEvent(IBloc bloc, object @event)
        {
            base.OnEvent(bloc, @event);
            Debug.Log($"{bloc.GetType().Name} {@event}");
        }

        public override void OnChange(IBloc bloc, IChange change)
        {
            base.OnChange(bloc, change);
            Debug.Log($"{bloc.GetType().Name} {change}");
        }

        public override void OnError(IBloc bloc, Exception error)
        {
            base.OnError(bloc, error);
            Debug.Log($"{bloc.GetType().Name} {error}");
        }

        public override void OnTransition(IBloc bloc, ITransition transition)
        {
            base.OnTransition(bloc, transition);
            Debug.Log($"{bloc.GetType().Name} {transition}");
        }
    }
}