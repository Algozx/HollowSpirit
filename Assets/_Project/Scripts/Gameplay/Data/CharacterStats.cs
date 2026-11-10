using UnityEngine;

namespace HollowSpirit.Gameplay.Data
{
    /// <summary>Health and stamina tuning for a character.</summary>
    [CreateAssetMenu(menuName = "HollowSpirit/Character Stats")]
    public sealed class CharacterStats : ScriptableObject
    {
        public float maxHealth;
        public float maxStamina;
        [Tooltip("Stamina regained per second once the delay has passed.")]
        public float staminaRegenPerSecond;
        [Tooltip("Seconds without spending stamina before regeneration starts.")]
        public float staminaRegenDelaySeconds;
        [Tooltip("Degrees per second the character turns toward the move direction.")]
        public float turnSpeedDegrees;
        [Tooltip("Seconds the locomotion speed takes to follow the stick.")]
        public float speedDampSeconds;
        [Tooltip("Downward speed applied while grounded so the controller stays on slopes and stairs.")]
        public float groundStickSpeed;
    }
}
