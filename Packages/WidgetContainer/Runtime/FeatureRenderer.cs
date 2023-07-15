using System;
using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    [RequireComponent(typeof(WidgetContainer))]
    public class FeatureRenderer : MonoBehaviour
    {
        [SerializeField] private WidgetContainer container;

        private void Reset()
        {
            container = GetComponent<WidgetContainer>();
        }
    }
}