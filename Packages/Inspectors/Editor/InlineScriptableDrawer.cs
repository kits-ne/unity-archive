using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inspectors
{
    [CustomPropertyDrawer(typeof(InlineScriptableAttribute))]
    public class InlineScriptableDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                return new PropertyField(property);
            }

            var (foldout, foldoutInput) = CreateFoldout();
            var propertyField = CreatePropertyField(property);
            foldoutInput.Add(propertyField);

            propertyField.RegisterValueChangeCallback(e =>
            {
                if (e.changedProperty.objectReferenceValue is not ScriptableObject scriptableObject)
                {
                    foldout.Clear();
                    foldout.value = false;
                }
                else
                {
                    foldout.Add(CreateScriptableInspector(scriptableObject));
                }
            });

            return foldout;
        }

        private PropertyField CreatePropertyField(SerializedProperty property)
        {
            var field = new PropertyField(property)
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = -4
                }
            };
            field.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
            {
                var display = field.Q(className: "unity-object-field-display");
                display.RegisterCallback<MouseDownEvent>(e => { e.StopPropagation(); });
            });
            return field;
        }

        private (Foldout foldout, VisualElement foldoutInput) CreateFoldout()
        {
            var foldout = new Foldout
            {
                value = false,
                viewDataKey = "inline-scriptable-foldout"
            };
            var foldoutInput = foldout.Q(className: "unity-foldout__input");
            return (foldout, foldoutInput);
        }

        private static VisualElement CreateScriptableInspector(ScriptableObject scriptableObject)
        {
            var serializedObject = new SerializedObject(scriptableObject);
            var container = new VisualElement();
            container.Bind(serializedObject);
            InspectorElement.FillDefaultInspector(container, serializedObject, null);
            return container;
        }
    }
}