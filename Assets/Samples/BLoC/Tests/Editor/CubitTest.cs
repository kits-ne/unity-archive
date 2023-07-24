using System;
using System.Collections;
using NUnit.Framework;
using UniBloc;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Samples.BLoC.Tests.Editor
{
    public class CounterCubit : Cubit<int>
    {
        public CounterCubit(int initialState) : base(initialState)
        {
        }

        public void Increment()
        {
            Emit(State + 1);
        }

        public void IncrementWithError()
        {
            AddError(new Exception("increment error"));
            Emit(State + 1);
        }

        protected override void OnChange(Change<int> change)
        {
            Debug.Log(change);
        }

        protected override void OnError(Exception error)
        {
            Debug.Log(error);
        }
    }

    public class CubitTest
    {
        /// <summary>
        /// https://bloclibrary.dev/#/ko-kr/coreconcepts?id=%ea%b8%b0%eb%b3%b8-%ec%82%ac%ec%9a%a9%eb%b2%95
        /// </summary>
        [NUnit.Framework.Test]
        public void CounterCubitBasicPasses()
        {
            var cubit = new CounterCubit(0);
            Debug.Log(cubit.State);
            cubit.Increment();
            Debug.Log(cubit.State);
            cubit.DisposeAsync();
        }

        /// <summary>
        /// https://bloclibrary.dev/#/ko-kr/coreconcepts?id=stream-%ec%82%ac%ec%9a%a9%eb%b2%95
        /// </summary>
        /// <returns></returns>
        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator CounterCubitStreamPasses()
        {
            var cubit = new CounterCubit(0);
            var subscription = cubit.Stream.Subscribe(_ => Debug.Log(_.ToString()));
            cubit.Increment();
            yield return null;
            subscription.Dispose();
            cubit.DisposeAsync();
        }

        [Test]
        public void CounterCubitErrorPasses()
        {
            Bloc.Observer = new SimpleBlocObserver();
            var cubit = new CounterCubit(0);
            var subscription = cubit.Stream.Subscribe(_ => Debug.Log(_.ToString()));
            cubit.IncrementWithError();
            subscription.Dispose();
            cubit.DisposeAsync();
        }
    }
}