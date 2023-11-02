using UnityEngine;

namespace UIToolbox
{
    public static class RectTransformExtensions
    {
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            var matrix = rectTransform.localToWorldMatrix;
            rect.min = matrix.MultiplyPoint(rect.min);
            rect.max = matrix.MultiplyPoint(rect.max);
            return rect;
        }
    }
}