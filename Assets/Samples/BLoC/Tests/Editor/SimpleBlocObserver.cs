using System;
using System.Diagnostics;
using UniBloc;
using Debug = UnityEngine.Debug;

namespace Samples.BLoC.Tests.Editor
{
    public class SimpleBlocObserver : BlocObserver
    {
        // ReSharper disable Unity.PerformanceAnalysis
        [Conditional("UNITY_EDITOR")]
        private void Log(string message)
        {
            Debug.Log(message);
        }

        public override void OnEvent(IBloc bloc, object @event)
        {
            base.OnEvent(bloc, @event);
            Log($"{bloc.GetType().Name} {@event}");
        }

        public override void OnChange(IBloc bloc, IChange change)
        {
            base.OnChange(bloc, change);
            Log($"{bloc.GetType().Name} {change}");
        }

        public override void OnError(IBloc bloc, Exception error)
        {
            base.OnError(bloc, error);
            Log($"{bloc.GetType().Name} {error}");
        }

        public override void OnTransition(IBloc bloc, ITransition transition)
        {
            base.OnTransition(bloc, transition);
            Log($"{bloc.GetType().Name} {transition}");
        }
    }
}