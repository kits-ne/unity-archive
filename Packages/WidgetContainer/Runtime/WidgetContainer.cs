using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime.Samples.WidgetContainer
{
    public interface IWidgetFactory
    {
        IWidget CreateWidget(string id);
    }

    public class TextLoggerFactory : IWidgetFactory
    {
        public IWidget CreateWidget(string id)
        {
            return new TextLogger()
            {
                Id = id
            };
        }
    }

    public class WidgetContainer : MonoBehaviour
    {
        [SerializeField] private List<WidgetInfo> widgets = new List<WidgetInfo>();

        private Dictionary<string, WidgetInfo> _widgetLookup;
        private readonly Dictionary<string, IWidget> _nullableWidgets = new Dictionary<string, IWidget>();

        private readonly Dictionary<Type, IWidgetFactory> _nullableWidgetTypes = new Dictionary<Type, IWidgetFactory>()
        {
            {typeof(IText), new TextLoggerFactory()}
        };

        public void Initialize()
        {
            _widgetLookup = widgets.ToDictionary(_ => _.id);
        }

        public T GetWidget<T>(string id) where T : class, IWidget
        {
            if (_widgetLookup.TryGetValue(id, out var value) && value.widget is T widget)
            {
                return widget;
            }

            if (!_nullableWidgets.TryGetValue(id, out var nullable)
                && _nullableWidgetTypes.TryGetValue(typeof(T), out var factory))
            {
                nullable = factory.CreateWidget(id);
                _nullableWidgets[id] = nullable;
            }

            return nullable as T;
        }

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

    public class TextLogger : IText
    {
        public string Id { get; set; }
        public bool IsActive { get; set; }

        private string _text;

        public string Text
        {
            get
            {
                Debug.Log($"text#{Id}: {_text}");
                return _text;
            }
            set
            {
                _text = value;
                Debug.Log($"text#{Id}: {_text}");
            }
        }
    }

    public interface ITextButton : IWidget, IText
    {
        Action Clicked { get; set; }
    }
}