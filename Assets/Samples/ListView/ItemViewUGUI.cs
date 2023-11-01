using Inspectors.DataBinding;
using UnityEngine;

namespace Samples.ListView
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ItemViewUGUI : ItemView
    {
        [SerializeField] private RectTransform rectTransform;
        public RectTransform RectTransform => rectTransform;

        public Vector2 Position
        {
            get => rectTransform.anchoredPosition;
            set => rectTransform.anchoredPosition = value;
        }

        public Vector2 Size => rectTransform.rect.size;

        public void SetAsFirstSibling() => rectTransform.SetAsFirstSibling();
        public void SetAsLastSibling() => rectTransform.SetAsLastSibling();

        public float Bottom => Position.y - Size.y * 0.5f; // TODO: pivot
        public Rect GetWorldRect() => rectTransform.GetWorldRect();

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }
}