using TMPro;
using UniBloc.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace UniBloc.Samples.Counter
{
    public class CounterWidget : PooledBlocWidget<CounterBloc, CounterEvent, int>
    {
        [SerializeField] private Button decrementButton;
        [SerializeField] private Button incrementButton;
        [SerializeField] private TextMeshProUGUI countLabel;

        protected override void OnCreated()
        {
            OnClick<CounterEvent.Decrement>(decrementButton);
            OnClick<CounterEvent.Increment>(incrementButton);
        }

        private readonly float _duration = 1;
        private float _timer = 0;

        public void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _duration)
            {
                Add<CounterEvent.Decrement>();
                _timer = 0;
            }
        }

        protected override void Render(int state)
        {
            countLabel.text = state.ToString();
        }
    }
}