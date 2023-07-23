using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    [CreateAssetMenu(menuName = "ScriptableFeature/CounterFeature", fileName = "CounterFeature", order = 0)]
    public class CounterFeature : ScriptableFeature
    {
        public override void Render(WidgetContainer container)
        {
            
            var countText = container.GetWidget<IText>("count-text");
            countText.Text = Random.Range(0, 100).ToString();

            var minusButton = container.GetWidget<ITextButton>("minus-button");
            minusButton.Text = "-";

            var plusButton = container.GetWidget<ITextButton>("plus-button");
            plusButton.Text = "+";
        }
    }
}