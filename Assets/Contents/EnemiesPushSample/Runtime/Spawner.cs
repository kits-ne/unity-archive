using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;
    [SerializeField] private List<Preset> presets;
    [SerializeField] private Rect bounds;

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < presets.Count; i++)
        {
            var p = presets[i];
            if (Input.GetKeyDown(p.keyCode))
            {
                for (int j = 0; j < p.count; j++)
                {
                    var e = Instantiate(prefab, parent).GetComponent<Enemy>();
                    var pos = new Vector2(
                        Random.Range(bounds.xMin, bounds.xMax),
                        Random.Range(bounds.yMin, bounds.yMax)
                    );
                    e.transform.position = pos;
                    e.target = FindObjectOfType<Player>().transform;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        var lt = bounds.min;
        var rt = new Vector2(
            bounds.xMax, bounds.yMin
        );

        var rb = bounds.max;
        var lb = new Vector2(
            bounds.xMin,
            bounds.yMax
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(lt, rt);
        Gizmos.DrawLine(rt, rb);
        Gizmos.DrawLine(rb, lb);
        Gizmos.DrawLine(lb, lt);
    }

    [Serializable]
    public class Preset
    {
        public KeyCode keyCode;
        public int count;
    }
}