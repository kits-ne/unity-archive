using UnityEngine;

namespace Samples.Inspectors
{
    [CreateAssetMenu(menuName = "Samples/Inspectors/Sample Data", fileName = "SampleData", order = 0)]
    public class SampleData : ScriptableObject
    {
        public int level = 1;
        public float exp = 1;
    }
}