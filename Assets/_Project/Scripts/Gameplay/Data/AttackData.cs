using HollowSpirit.Gameplay.Animation;
using UnityEngine;

namespace HollowSpirit.Gameplay.Data
{
    /// <summary>Attack tuning: stamina cost, damage and the damage window.</summary>
    [CreateAssetMenu(menuName = "HollowSpirit/Attack Data")]
    public sealed class AttackData : ScriptableObject, IActionWindowSource
    {
        public float staminaCost;
        public float damage;
        [Tooltip("Seconds after the attack starts when the blade begins to hurt.")]
        public float damageStartSeconds;
        [Tooltip("Seconds after the attack starts when the blade stops hurting.")]
        public float damageEndSeconds;

        public float WindowStartSeconds => damageStartSeconds;
        public float WindowEndSeconds => damageEndSeconds;
    }
}
