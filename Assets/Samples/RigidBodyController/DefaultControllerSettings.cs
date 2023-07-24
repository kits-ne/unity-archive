using UnityEngine;

namespace Samples.RigidBodyController
{
    public class DefaultControllerSettings : IControllerSettings
    {
        public static readonly DefaultControllerSettings Instance = new();
        public float MaxSpeed => 30;
        public Vector2 Acceleration => Vector2.one * 20;
        public float Deceleration => 50;
        public float DashVelocity => 30;
        public int DashFrames => 10;
        public float DashEndMultiplier => 0.3f;

        private DefaultControllerSettings()
        {
        }
    }
}