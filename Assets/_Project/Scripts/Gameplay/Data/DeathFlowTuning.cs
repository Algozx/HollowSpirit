using UnityEngine;

namespace HollowSpirit.Gameplay.Data
{
    /// <summary>Timing of the player's death and respawn.</summary>
    [CreateAssetMenu(menuName = "HollowSpirit/Death Flow Tuning")]
    public sealed class DeathFlowTuning : ScriptableObject
    {
        [Tooltip("Seconds between the player's death and the respawn.")]
        public float respawnDelaySeconds;
    }
}
