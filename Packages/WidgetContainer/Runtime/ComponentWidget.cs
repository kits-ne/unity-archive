using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    public class ComponentWidget : MonoBehaviour, IWidget
    {
        public bool IsActive
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }
    }
}