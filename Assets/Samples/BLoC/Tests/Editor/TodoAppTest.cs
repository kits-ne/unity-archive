using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using NUnit.Framework;
using UniBloc;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Samples.BLoC.Tests.Editor
{
    public class TodoAppTest
    {
        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator RepoForEachAsyncPasses() => UniTask.ToCoroutine(async () =>
        {
            var repo = new TodoRepo();
            await repo.ProductIdeas().ForEachAsync(Debug.Log);
        });

        [UnityTest]
        public IEnumerator TodoAppTestWithEnumeratorPasses() => UniTask.ToCoroutine(async () =>
        {
            var bloc = new TodoAppBloc(new TodoRepo());
            var subscription = bloc.Stream.Subscribe(Debug.Log);
            bloc.Add(new TodoReadEvent());

            await UniTask.Delay(TimeSpan.FromSeconds(6));
            subscription.Dispose();

            subscription = bloc.Stream.Subscribe(Debug.Log);
            bloc.Add(new TodoReadEvent());
            await UniTask.Delay(TimeSpan.FromSeconds(6));
            subscription.Dispose();

            await bloc.DisposeAsync();
        });
    }

    public class TodoAppBloc : Bloc<TodoEvent, TodoState>
    {
        private readonly TodoRepo _repo;

        public TodoAppBloc(TodoRepo repo) : base(new TodoState())
        {
            _repo = repo;
            On<TodoReadEvent>((e, emitter) =>
            {
                return emitter.ForEach(new Stream<string>(_repo.ProductIdeas()), onData: (data) => new TodoState()
                {
                    Todo = data
                });
            });
        }
    }

    public class TodoEvent : EventBase<TodoEvent>
    {
    }

    public sealed class TodoReadEvent : TodoEvent
    {
    }

    public class TodoState : IEquatable<TodoState>
    {
        public string Todo;
        public override string ToString() => Todo;

        public bool Equals(TodoState other) => this == other;
    }

    public class TodoRepo
    {
        int _currentAppIdea = 0;

        private readonly string[] _ideas =
        {
            "Future prediction app that rewards you if you predict the next day's news",
            "Dating app for fish that lets your aquarium occupants find true love",
            "Social media app that pays you when your data is sold",
            "JavaScript framework gambling app that lets you bet on the next big thing",
            "Solitaire app that freezes before you can win",
        };

        public IUniTaskAsyncEnumerable<string> ProductIdeas()
        {
            return _ideas.ToUniTaskAsyncEnumerable().SelectAwait(async (str) =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                return str;
            });
        }
    }
}