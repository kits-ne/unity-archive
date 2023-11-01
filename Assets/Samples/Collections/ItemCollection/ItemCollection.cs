using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;
using System.Collections.Generic;

namespace DefaultCompany.Samples.Collections
{
    [CreateAssetMenu(menuName = "ScriptableObject Collection/Collections/Create ItemCollection", fileName = "ItemCollection", order = 0)]
    public class ItemCollection : ScriptableObjectCollection<Item>
    {
    }
}
