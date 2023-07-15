using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Samples.WidgetContainer
{
    [RequireComponent(typeof(Button))]
    public class UGUIButton : ComponentWidget, ITextButton
    {
        [SerializeField] [Shortcut("Text")] private UGUIText text;
        [SerializeField] private Button button;

        public string Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        public Action Clicked { get; set; }

        private void Reset()
        {
            button ??= GetComponent<Button>();
        }
    }
}