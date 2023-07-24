using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
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

        protected override void Render(int state)
        {
            countLabel.text = state.ToString();
        }
    }
}