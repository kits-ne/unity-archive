using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Samples.BLoC;
using TMPro;
using UniBloc;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CounterWidget : MonoBehaviour
{
    public TextMeshProUGUI countText;
    public Button decrementButton;
    public Button incrementButton;

    public TextMeshProUGUI levelText;
    public Button levelAddButton;

    private CounterBloc _counterBloc;
    private LevelBloc _levelBloc;

    void Start()
    {
        _counterBloc = new CounterBloc();
        countText.text = _counterBloc.State.ToString();
        _counterBloc.Stream.BindTo(countText);

        var token = destroyCancellationToken;
        decrementButton.OnClickAsAsyncEnumerable().Subscribe(_ => _counterBloc.Add<CounterEvent.Decrement>(), token);
        incrementButton.OnClickAsAsyncEnumerable().Subscribe(_ => _counterBloc.Add<CounterEvent.Increment>(), token);

        _levelBloc = new LevelBloc(new LevelState()
        {
            Level = 12
        });
        levelText.text = _levelBloc.State.ToString();
        _levelBloc.Stream.BindTo(levelText);
        levelAddButton.OnClickAsAsyncEnumerable()
            .Subscribe(_ => { _levelBloc.Add<LevelEvent.Add>(e => { e.Amount = Random.Range(1, 10); }); }, token);
        
    }

    private void OnDestroy()
    {
        UniTask.WhenAll(
            _levelBloc.DisposeAsync(),
            _counterBloc.DisposeAsync()
        ).ContinueWith(() =>
        {
            Debug.Log("disposed");
        });
    }

    #region Counter

    public class CounterBloc : PooledBloc<CounterEvent, int>
    {
        public CounterBloc() : base(0)
        {
            On<CounterEvent.Increment>(async (e, emitter) => emitter.Emit(State + 1));
            On<CounterEvent.Decrement>(async (e, emitter) => emitter.Emit(State - 1));
        }
    }

    public class CounterEvent : EventBase<CounterEvent>
    {
        public sealed class Increment : CounterEvent
        {
            public override string ToString() => nameof(Increment);
        }

        public sealed class Decrement : CounterEvent
        {
            public override string ToString() => nameof(Decrement);
        }
    }

    #endregion

    #region Level

    public class LevelBloc : PooledBloc<LevelEvent, LevelState>
    {
        public LevelBloc(LevelState initialState) : base(initialState)
        {
            UsingStatePool();
            On<LevelEvent.Add>((e, emitter) =>
            {
                var state = GetState();
                state.Level = State.Level + e.Amount;
                emitter.Emit(state);
            });
        }

        protected override void OnTransition(Transition<LevelEvent, LevelState> transition)
        {
            Debug.Log(transition);
        }
    }

    public class LevelEvent : EventBase<LevelEvent>
    {
        public sealed class Add : LevelEvent
        {
            public int Amount = 1;
            public override string ToString() => $"{{ {nameof(Add)}: {Amount} }}";
        }
    }

    public sealed record LevelState
    {
        public int Level;
        public override string ToString() => Level.ToString();
    }

    #endregion
}