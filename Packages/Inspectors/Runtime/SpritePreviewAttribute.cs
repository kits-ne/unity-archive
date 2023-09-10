using System;
using UnityEngine;

namespace Inspectors
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SpritePreviewAttribute : PropertyAttribute
    {
        public readonly int Height = 0;

        public SpritePreviewAttribute()
        {
        }

        public SpritePreviewAttribute(int height)
        {
            Height = height;
        }
    }
}