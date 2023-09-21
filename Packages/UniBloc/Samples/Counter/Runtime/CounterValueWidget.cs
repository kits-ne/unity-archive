using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UniBloc.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace UniBloc.Samples.Counter
{
    public class CounterValueWidget : ValueBlocWidget<CounterValueBloc, CounterEventID, CounterValueEvent, int>
    {
        [SerializeField] private Button decrementButton;
        [SerializeField] private Button incrementButton;
        [SerializeField] private TextMeshProUGUI countLabel;

        private readonly float _duration = 1;
        private float _timer = 0;

        protected override void OnCreated()
        {
            decrementButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => { Add(new(CounterValueEvent.Decrement)); })
                .AddTo(destroyCancellationToken);
            incrementButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => { Add(new(CounterValueEvent.Increment)); })
                .AddTo(destroyCancellationToken);
        }

        public void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _duration)
            {
                Add(new(CounterValueEvent.Decrement));
                _timer = 0;
            }
        }

        protected override void Render(int state)
        {
            countLabel.text = state.ToString();
        }
    }

    public class CounterValueBloc : ValueBloc<CounterEventID, CounterValueEvent, int>
    {
        public CounterValueBloc() : base(0)
        {
            On(new(CounterValueEvent.Decrement), (e, emitter) => { emitter.Emit(State - 1); });
            On(new(CounterValueEvent.Increment), (e, emitter) =>
            {
                if (State >= 10)
                {
                    AddError(new Exception($"overflow error: {State}"));
                    return;
                }

                emitter.Emit(State + 1);
            });
            On(new(CounterValueEvent.Reset), (e, emitter) => emitter.Emit(0));
        }

        protected override void OnError(Exception error)
        {
            Add(new(CounterValueEvent.Reset));
        }
    }

    public readonly struct CounterValueEvent : IEventEntity<CounterEventID>, IEquatable<CounterValueEvent>
    {
        public static readonly CounterEventID Increment = CounterEventID.Create();
        public static readonly CounterEventID Decrement = CounterEventID.Create();
        public static readonly CounterEventID Reset = CounterEventID.Create();

        public CounterValueEvent(CounterEventID id)
        {
            ID = id;
        }

        public CounterEventID ID { get; }

        public bool Equals(CounterValueEvent other) => GetHashCode() == other.GetHashCode();

        public override int GetHashCode() => ID.GetHashCode();
    }

    public readonly struct CounterEventID : IEquatable<CounterEventID>
    {
        private static long _nextID = 1;
        private readonly long _id;
        public bool Equals(CounterEventID other) => GetHashCode() == other.GetHashCode();
        public override int GetHashCode() => _id.GetHashCode();

        private CounterEventID(long id)
        {
            _id = id;
        }

        public static CounterEventID Create() => new(_nextID++);
    }
}