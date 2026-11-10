using HollowSpirit.Gameplay.Animation;
using UnityEngine;

namespace HollowSpirit.Gameplay.Data
{
    /// <summary>Roll tuning: stamina cost and the invulnerability window.</summary>
    [CreateAssetMenu(menuName = "HollowSpirit/Roll Data")]
    public sealed class RollData : ScriptableObject, IActionWindowSource
    {
        public float staminaCost;
        [Tooltip("Seconds after the roll starts when invulnerability begins.")]
        public float iFrameStartSeconds;
        [Tooltip("Seconds after the roll starts when invulnerability ends.")]
        public float iFrameEndSeconds;

        public float WindowStartSeconds => iFrameStartSeconds;
        public float WindowEndSeconds => iFrameEndSeconds;
    }
}
