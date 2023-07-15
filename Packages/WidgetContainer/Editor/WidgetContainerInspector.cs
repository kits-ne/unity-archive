using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Runtime.Samples.WidgetContainer.Editor
{
    [CustomEditor(typeof(WidgetContainer))]
    public class WidgetContainerInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            InspectorElement.FillDefaultInspector(container, serializedObject, this);
            if (target is not WidgetContainer widgetContainer) return container;

            var collectButton = new Button(() =>
            {
                widgetContainer.Collect();
                EditorUtility.SetDirty(widgetContainer);
            })
            {
                text = "Collect",
                style = {height = 50}
            };
            container.Insert(0, collectButton);

            return container;
        }
    }
}