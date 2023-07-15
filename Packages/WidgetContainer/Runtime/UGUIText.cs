using TMPro;
using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UGUIText : ComponentWidget, IText
    {
        [SerializeField] [Shortcut("text")] private TextMeshProUGUI text;
    
        public string Text
        {
            get => text.text;
            set => text.text = value;
        }

        private void Reset()
        {
            text = GetComponent<TextMeshProUGUI>();
        }
    }
}