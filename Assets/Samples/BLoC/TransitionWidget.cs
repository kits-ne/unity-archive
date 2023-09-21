using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniBloc;
using UniBloc.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Samples.BLoC
{
    public class TransitionWidget : ValueBlocWidget<TransitionBloc, int, TransitionEvent, TransitionState>,
        IPointerDownHandler
    {
        [SerializeField] private CanvasGroup group;

        protected override void OnCreated()
        {
        }

        protected override void Render(TransitionState state)
        {
            group.alpha = state.Progress;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Add(new TransitionEvent(1));
        }
    }

    public class TransitionBloc : ValueBloc<int, TransitionEvent, TransitionState>
    {
        private readonly ChannelController<float> _progressChannel;

        public TransitionBloc() : base(new(1))
        {
            _progressChannel = new();
            On(1, (e, emitter) =>
            {
                if (State.Progress < 1) return UniTask.CompletedTask;

                return emitter.ForEach(new Stream<float>(PlayProgress()), v => new(v));
            });
        }

        private IUniTaskAsyncEnumerable<float> PlayProgress()
        {
            return UniTaskAsyncEnumerable.Create<float>(async (writer, token) =>
            {
                await UniTask.Yield();
                var t = 0f;
                while (!token.IsCancellationRequested && !(t > 1))
                {
                    t += Time.deltaTime;
                    await writer.YieldAsync(t);
                    await UniTask.Yield();
                }
            });
        }

        public override UniTask DisposeAsync()
        {
            _progressChannel.Dispose();
            return base.DisposeAsync();
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