using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UI;
using VYaml.Annotations;
using VYaml.Serialization;
using Random = UnityEngine.Random;

namespace Samples.UIImporter
{
    [ScriptedImporter(0, "ui.yml")]
    public class UITextImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var texts = File.ReadAllBytes(ctx.assetPath);

            var widget = YamlSerializer.Deserialize<WidgetRoot>(texts);

            var root = new GameObject(Path.GetFileName(ctx.assetPath));

            if (widget.Children != null)
            {
                var vertical = root.AddComponent<VerticalLayoutGroup>();
                var index = 0;
                foreach (var child in widget.Children)
                {
                    var go = new GameObject(string.IsNullOrEmpty(child.Name)
                        ? (index++).ToString()
                        : child.Name);

                    switch (child.Type)
                    {
                        case "text":
                            var tm = go.AddComponent<TextMeshProUGUI>();
                            tm.text = child.Value;
                            break;
                    }

                    go.transform.SetParent(root.transform);
                }
            }

            ctx.AddObjectToAsset("main", root);
            ctx.SetMainObject(root);
        }
    }

    [YamlObject]
    public partial class WidgetRoot
    {
        public List<Widget> Children;
    }

    [YamlObject]
    public partial class Widget
    {
        public List<Widget> Children;
        public string Type;
        public string Name;
        public string Value;
    }
}