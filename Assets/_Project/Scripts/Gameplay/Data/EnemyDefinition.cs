using UnityEngine;

namespace HollowSpirit.Gameplay.Data
{
    /// <summary>Tuning of one enemy type: health, souls dropped, senses and attacks.</summary>
    [CreateAssetMenu(menuName = "HollowSpirit/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public float maxHealth;
        [Tooltip("Souls the enemy drops when it dies.")]
        public int souls;
        [Tooltip("Distance at which the enemy notices the player, in metres.")]
        public float sightRange;
        [Tooltip("Distance at which the enemy stops chasing and attacks, in metres; what the blade can reach.")]
        public float attackRange;
        [Tooltip("The agent stops this much short of the attack range so it arrives inside it, in metres.")]
        public float arrivalToleranceMetres;
        [Tooltip("The swing ends to one side of the body, so the enemy faces this many degrees to the left of the player when it attacks.")]
        public float attackAimOffsetDegrees;
        [Tooltip("Shortest wait between two attacks, in seconds.")]
        public float attackCooldownMinSeconds;
        [Tooltip("Longest wait between two attacks, in seconds.")]
        public float attackCooldownMaxSeconds;
        [Tooltip("Attacks the enemy can use; the first one is used for now.")]
        public AttackData[] attacks;
    }
}
