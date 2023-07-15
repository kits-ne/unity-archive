using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Runtime.Samples.WidgetContainer.Editor
{
    [CustomEditor(typeof(Object), true, isFallback = true)]
    public class DefaultInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            // IMGUI同様のInspectorを実装
            InspectorElement.FillDefaultInspector(container, serializedObject, this);

            return container;
        }
    }
}