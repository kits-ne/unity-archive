using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniBloc;
using UniBloc.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Samples.BLoC
{
    public class TransitionWidget : ValueBlocWidget<TransitionBloc, int, TransitionEvent, TransitionState>,
        IPointerDownHandler
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private ConcurrencyMode mode;

        protected override TransitionBloc CreateBloc()
        {
            return new(mode);
        }

        protected override void OnCreated()
        {
        }

        protected override void Render(TransitionState state)
        {
            group.alpha = state.Progress;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Add(new TransitionEvent(2));
        }
    }


    public class TransitionBloc : ValueBloc<int, TransitionEvent, TransitionState>
    {
        public TransitionBloc(ConcurrencyMode mode) : base(new(1))
        {
            On(1, async (e, emitter, token) =>
            {
                Debug.Log("in");
                await emitter.ForEach(
                    PlayProgressAsync(token).ToStream(),
                    v => new(v));
                Debug.Log("out");
            }, mode);
            On(2, async (e, emitter, token) =>
            {
                Debug.Log("in");
                await emitter.ForEach(PlayProgress(token).ToStream(),
                    v => new(v));
                Debug.Log("out");
            }, mode);
        }

        public TransitionBloc() : base(new(1))
        {
        }

        private async IAsyncEnumerable<float> PlayProgressAsync(
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await UniTask.Yield();
            var t = 0f;
            while (!token.IsCancellationRequested)
            {
                t += Time.deltaTime;
                Debug.Log(t);
                yield return t;
                await UniTask.Yield();
            }

            Debug.Log("end");
        }

        private async IAsyncEnumerable<float> PlayProgress(
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await UniTask.Yield();
            var t = 0f;
            while (!token.IsCancellationRequested && t < 1)
            {
                t += Time.deltaTime;
                // Debug.Log(t);
                yield return t;
                await UniTask.Yield();
            }

            Debug.Log("end");
        }
    }

    public readonly struct TransitionEvent : IEventEntity<int>, IEquatable<TransitionEvent>
    {
        public TransitionEvent(int id)
        {
            ID = id;
        }

        public int ID { get; }
        public bool Equals(TransitionEvent other) => GetHashCode() == other.GetHashCode();
        public override int GetHashCode() => ID.GetHashCode();
    }

    public readonly struct TransitionState : IEquatable<TransitionState>
    {
        public readonly float Progress;

        public TransitionState(float progress)
        {
            Progress = progress;
        }

        public bool Equals(TransitionState other) => GetHashCode() == other.GetHashCode();

        public override int GetHashCode() => Progress.GetHashCode();
    }
}