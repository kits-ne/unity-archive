using System.Runtime.CompilerServices;
using UnityEngine;

namespace UIToolbox
{
    public static class RectUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool In(float value, float min, float max) => min <= value && value <= max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Out(float value, float min, float max) => value < min || max < value;

        public static ViewState GetViewState(Rect a, Rect b)
        {
            if (In(b.xMin, a.xMin, a.xMax) && In(b.xMax, a.xMin, a.xMax) &&
                In(b.yMin, a.yMin, a.yMax) && In(b.yMax, a.yMin, a.yMax))
            {
                return ViewState.InView;
            }

            if (Out(b.xMin, a.xMin, a.xMax) && Out(b.xMax, a.xMin, a.xMax) ||
                Out(b.yMin, a.yMin, a.yMax) && Out(b.yMax, a.yMin, a.yMax))
            {
                return ViewState.OutOfView;
            }

            return ViewState.Partial;
        }

        public enum ViewState
        {
            Partial,
            InView,
            OutOfView
        };
    }
}