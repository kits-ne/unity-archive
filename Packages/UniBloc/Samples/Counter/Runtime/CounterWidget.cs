using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniBloc.Samples.Counter
{
    public class CounterWidget : MonoBehaviour
    {
        [SerializeField] private Button decrementButton;
        [SerializeField] private Button incrementButton;
        [SerializeField] private TextMeshProUGUI countLabel;

        private CounterBloc _counter;

        private void Awake()
        {
            _counter = new CounterBloc();

            var token = this.GetCancellationTokenOnDestroy();
            _counter.Stream.Subscribe(Render).AddTo(token);
            decrementButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => _counter.Add<CounterEvent.Decrement>())
                .AddTo(token);
            incrementButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => _counter.Add<CounterEvent.Increment>())
                .AddTo(token);

            Render(_counter.State);
        }

        private void Render(int state)
        {
            countLabel.text = state.ToString();
        }

        private void OnDestroy()
        {
            _counter?.DisposeAsync();
        }
    }
}