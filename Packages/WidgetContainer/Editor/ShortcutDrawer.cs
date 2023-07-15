using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runtime.Samples.WidgetContainer.Editor
{
    [CustomPropertyDrawer(typeof(ShortcutAttribute))]
    public class ShortcutDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (attribute is not ShortcutAttribute shortcut) return new PropertyField(property);

            var root = new VisualElement();
            root.Add(new PropertyField(property));

            var prop = property.objectReferenceValue.GetType().GetProperty(shortcut.Name);
            if (prop != null)
            {
                if (prop.PropertyType == typeof(string))
                {
                    var field = new TextField()
                    {
                        // isDelayed = true
                    };
                    field.RegisterValueChangedCallback(e =>
                    {
                        prop.SetValue(property.objectReferenceValue, e.newValue);
                    });
                    field.SetValueWithoutNotify((string) prop.GetValue(property.objectReferenceValue));

                    root.Add(field);
                }
            }

            return root;
        }
    }
}