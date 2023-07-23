using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    public abstract class ScriptableFeature : ScriptableObject
    {
        public abstract void Render(WidgetContainer container);
    }
}