using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    public class WidgetContainer : MonoBehaviour
    {
        [SerializeField] private List<WidgetInfo> widgets = new List<WidgetInfo>();

        public void Collect()
        {
            widgets.Clear();

            var containers = GetComponentsInChildren<WidgetContainer>();
            foreach (var container in containers)
            {
                container.gameObject.SetActive(false);
            }

            var components = GetComponentsInChildren<ComponentWidget>(false);
            foreach (var widget in components)
            {
                var info = new WidgetInfo()
                {
                    widget = widget
                };
                if (widget.name.StartsWith("#"))
                {
                    info.id = widget.name[1..];
                }

                if (info.IsValid)
                {
                    widgets.Add(info);
                }
            }

            foreach (var container in containers)
            {
                container.gameObject.SetActive(true);
            }

            // string GetPath(ComponentWidget widget)
            // {
            //     paths.Clear();
            //     Transform current = widget.transform;
            //     do
            //     {
            //         paths.Add(current.name);
            //         current = current.parent;
            //     } while (current != null && current != transform);
            //
            //     paths.Reverse();
            //     return string.Join("/", paths);
            // }
        }
    }

    [Serializable]
    public class WidgetInfo
    {
        public string id;
        public ComponentWidget widget;

        public bool IsValid => !string.IsNullOrWhiteSpace(id) && widget;
    }

    public interface IWidget
    {
        bool IsActive { get; set; }
    }

    public interface IText : IWidget
    {
        string Text { get; set; }
    }

    public interface ITextButton : IWidget, IText
    {
        Action Clicked { get; set; }
    }
}