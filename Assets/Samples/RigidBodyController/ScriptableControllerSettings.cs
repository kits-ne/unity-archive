using UnityEngine;

namespace Samples.RigidBodyController
{
    [CreateAssetMenu(fileName = "Controller Settings", menuName = "2d Controller/Controller Settings", order = 0)]
    public class ScriptableControllerSettings : ScriptableObject, IControllerSettings
    {
        [field: SerializeField] public float MaxSpeed { get; private set; } = 30;
        [field: SerializeField] public Vector2 Acceleration { get; private set; } = Vector2.one * 20;
        [field: SerializeField] public float Deceleration { get; private set; } = 50;
        [field: SerializeField] public float DashVelocity { get; private set; } = 30;
        [field: SerializeField] public int DashFrames { get; private set; } = 10;
        [field: SerializeField] public float DashEndMultiplier { get; private set; } = 0.3f;
    }
}