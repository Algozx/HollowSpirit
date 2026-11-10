using UnityEngine;

namespace HollowSpirit.Gameplay.Data
{
    /// <summary>Lock-on tuning: how far and how wide the player can lock, and when the lock breaks.</summary>
    [CreateAssetMenu(menuName = "HollowSpirit/Lock-On Tuning")]
    public sealed class LockOnTuning : ScriptableObject
    {
        [Tooltip("Farthest distance from the player at which a target can be locked, in metres.")]
        public float maxDistance;
        [Tooltip("Widest angle from the camera forward at which a target can be locked, in degrees.")]
        public float maxViewAngleDegrees;
        [Tooltip("Distance from the player beyond which a lock is released, in metres.")]
        public float breakDistance;
    }
}
