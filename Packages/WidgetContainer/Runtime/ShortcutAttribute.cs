using System;
using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShortcutAttribute : PropertyAttribute
    {
        public string Name { get; private set; }

        public ShortcutAttribute(string name)
        {
            Name = name;
        }
    }
}