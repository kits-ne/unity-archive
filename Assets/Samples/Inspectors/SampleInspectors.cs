using System.Collections;
using System.Collections.Generic;
using Inspectors;
using Inspectors.DataBinding;
using UnityEngine;

namespace Samples.Inspectors
{
    [ItemVisuals(typeof(SubVisuals), typeof(SubVisuals2), typeof(SubVisuals3))]
    public class SampleInspectors : ItemView
    {
        [SpritePreview(150)] public Sprite sprite;

        [Find(".")] public SpriteRenderer selfSpriteRenderer;
        [Find] public BoxCollider selfBoxCollider;

        [Find("./")] public GameObject childA;

        [Find("./child-a")] public GameObject childA1;

        [Find("./child-a", nameof(childA1))] public SpriteRenderer childA1SpriteRenderer;
        [Find("./child-a", "child-a1")] public BoxCollider childA1BoxCollider;

        [Find("./child-c")] public GameObject childC1;
        [Find("./child-c", nameof(childC1))] public SpriteRenderer childC1SpriteRenderer;
    }


    public class SubVisuals : ItemVisuals
    {
        public int ID;
    }

    public class SubVisuals2 : ItemVisuals
    {
        public string Name;
    }

    public class SubVisuals3 : ItemVisuals
    {
        public Vector3 ID;
    }
}