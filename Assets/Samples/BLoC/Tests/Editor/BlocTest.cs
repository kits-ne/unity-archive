using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UniBloc;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Is = NUnit.Framework.Is;

namespace Samples.BLoC.Tests.Editor
{
    public class BlocTest
    {
        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator BlocBasicPasses() => UniTask.ToCoroutine(async () =>
        {
            Bloc.Observer = new SimpleBlocObserver();
            var bloc = new CounterBloc();
            Debug.Log(bloc.State);
            bloc.Add(new CounterIncrementEvent(2));
            Debug.Log(bloc.State);
            await bloc.DisposeAsync();
        });

        [UnityTest]
        public IEnumerator BlocStreamPasses() => UniTask.ToCoroutine(async () =>
        {
            var bloc = new CounterBloc();
            var subscription = bloc.Stream.Subscribe(_ => Debug.Log(_));
            bloc.Add(new CounterIncrementEvent(2));
            subscription.Dispose();
            await bloc.DisposeAsync();
        });


        [Test, Performance]
        public void BlocBasicPerformance()
        {
            Measure.Method(async () =>
                {
                    var bloc = new CounterBloc();
                    bloc.Add(new CounterIncrementEvent());
                    await bloc.DisposeAsync();
                })
                .MeasurementCount(1)
                .Run();
        }

        [Test]
        public void SettingAVariableDoesNotAllocate()
        {
            var list = new List<int>()
            {
                1, 2, 3, 4, 5
            };
            var msg = "test";

            Func<int, bool> predicate = (val) => val == 5;
            Assert.That(() =>
            {
                int a = 0;
                a = 1;
                bool isAny = list.AnyNonAlloc(predicate);
                var b = new Vector3()
                {
                    x = 10,
                    y = 33,
                    z = 393
                };
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void SimpleBlocTest()
        {
            var bloc = new SimpleBloc(new SimpleState()
            {
                Level = 10
            });
            bloc.Stream.Subscribe(Debug.Log);
            bloc.Add<SimpleEvent.Foo>();

            Assert.That(() =>
            {
                bloc.Add<SimpleEvent.Foo>();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void ListAnyAllocTest()
        {
            var list = new List<int>();

            bool isAny = list.Any();
        }
    }

    class SimpleEvent : EventBase<SimpleEvent>
    {
        public sealed class Foo : SimpleEvent
        {
        }
    }

    record SimpleState
    {
        public int Level;
    }

    class SimpleBloc : PooledBloc<SimpleEvent, SimpleState>
    {
        public SimpleBloc(SimpleState initialState) : base(initialState)
        {
            UsingStatePool();
            On<SimpleEvent.Foo>(async (e, emitter) =>
            {
                var state = GetState();
                state.Level = State.Level + 1;
                emitter.Emit(state);
            });
        }
    }

    public static class Extensions
    {
        public static bool AnyNonAlloc<T>(this List<T> list, Func<T, bool> predicate = null)
        {
            if (predicate == null)
            {
                return list.Count > 0;
            }

            foreach (var item in list)
            {
                if (predicate(item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}