using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inspectors
{
    [CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
    public class SpritePreviewPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                fieldInfo.FieldType == typeof(Sprite))
            {
                var container = new VisualElement();

                var field = new PropertyField(property);
                container.Add(field);

                var image = new VisualElement();
                container.Add(image);


                field.RegisterValueChangeCallback(e =>
                {
                    if (e.changedProperty.objectReferenceValue is Sprite sprite)
                    {
                        SetSprite(this, image, sprite);
                    }
                    else
                    {
                        image.style.backgroundImage = null;
                    }
                });

                if (property.objectReferenceValue != null && property.objectReferenceValue is Sprite spr)
                {
                    SetSprite(this, image, spr);
                }

                return container;
            }
            else
            {
                return new PropertyField(property);
            }
        }

        private void SetSprite(PropertyDrawer drawer, VisualElement container, Sprite sprite)
        {
            var height = 0;
            if (drawer.attribute is SpritePreviewAttribute previewAttribute)
            {
                height = previewAttribute.Height;
            }

            // var texture = new StyleBackground(AssetPreview.GetAssetPreview(sprite));
            container.style.height = height == 0 ? sprite.textureRect.height : height;
            // container.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            // container.style.UnityBackgroundScaleMode(ScaleMode.ScaleToFit);
            container.style.SetBackgroundPosition(BackgroundPositionKeyword.Center, BackgroundPositionKeyword.Top);
            container.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            // container.style.maxHeight = 200;
            container.style.backgroundImage = sprite.texture;
        }
    }

    public static class StyleExtensions
    {
        public static void SetBackgroundPosition(this IStyle style,
            BackgroundPositionKeyword x,
            BackgroundPositionKeyword y)
        {
            style.backgroundPositionX = new BackgroundPosition(x);
            style.backgroundPositionY = new BackgroundPosition(y);
        }

        public static void UnityBackgroundScaleMode(this IStyle style, ScaleMode mode)
        {
            if (mode == ScaleMode.ScaleToFit)
            {
                style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
        }
    }
}