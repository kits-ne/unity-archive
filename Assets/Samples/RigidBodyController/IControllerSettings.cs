using UnityEngine;

namespace Samples.RigidBodyController
{
    public interface IControllerSettings
    {
        float MaxSpeed { get; }
        Vector2 Acceleration { get; }
        float Deceleration { get; }
        float DashVelocity { get; }
        int DashFrames { get; }
        float DashEndMultiplier { get; }
    }
}