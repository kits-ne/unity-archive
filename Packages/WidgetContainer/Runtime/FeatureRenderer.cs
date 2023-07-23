using System;
using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    [RequireComponent(typeof(WidgetContainer))]
    public class FeatureRenderer : MonoBehaviour
    {
        [SerializeField] private WidgetContainer container;
        [SerializeField] private ScriptableFeature feature;

        private void Awake()
        {
            container.Initialize();
        }

        private void OnEnable()
        {
            feature.Render(container);
        }

        private void OnDisable()
        {
        }

        private void Reset()
        {
            container = GetComponent<WidgetContainer>();
        }
    }
}