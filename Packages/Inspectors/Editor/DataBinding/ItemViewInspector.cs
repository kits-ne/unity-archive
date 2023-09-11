using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inspectors.DataBinding.Editor
{
    [CustomEditor(typeof(ItemView), true)]
    public class ItemViewInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            if (target is not ItemView itemView) return root;

            var targetType = target.GetType();
            var visualsFieldInfo = targetType.BaseType?.GetField("visuals",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (visualsFieldInfo == null)
            {
                Debug.LogWarning("not found visuals field");
                return root;
            }

            var attr = targetType.GetCustomAttribute<ItemVisualsAttribute>();
            if (attr == null) return root;

            var visualsTypes = attr.Types;

            var defaultIndex = 0;
            if (!itemView.HasVisuals)
            {
                InstantiateVisuals(visualsFieldInfo, visualsTypes.First());
            }
            else
            {
                var currentVisualType = itemView.Visuals.GetType();
                defaultIndex = visualsTypes.IndexOf(currentVisualType);
            }

            var popupField = new PopupField<Type>(visualsTypes, defaultIndex,
                t => $"{t.Name} ({t.Namespace})");

            popupField.RegisterValueChangedCallback(e => { InstantiateVisuals(visualsFieldInfo, e.newValue); });
            root.Insert(1, popupField);
            return root;
        }

        private void InstantiateVisuals(FieldInfo visualsFieldInfo, Type visualsType)
        {
            var itemVisuals = Activator.CreateInstance(visualsType) as ItemVisuals;
            visualsFieldInfo.SetValue(target, itemVisuals);
        }
    }
}