using HollowSpirit.Gameplay.Data;
using UnityEngine;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Practice target: reacts to every hit, falls at zero health and stands back up at full health.</summary>
    [RequireComponent(typeof(Animator))]
    public sealed class TrainingDummy : MonoBehaviour, IDamageable
    {
        private static readonly int GetHitHash = Animator.StringToHash("GetHit");
        private static readonly int KnockdownHash = Animator.StringToHash("Knockdown");

        [SerializeField] private CharacterStats stats;

        private Animator animator;

        public Health Health { get; private set; }

        private void Awake()
        {
            if (stats == null)
            {
                Debug.LogError($"{nameof(TrainingDummy)}: stats are not assigned on {name}.", this);
                enabled = false;
                return;
            }

            animator = GetComponent<Animator>();
            Health = new Health(stats.maxHealth);
        }

        public bool TryTakeHit(in HitInfo hit)
        {
            if (Health == null || Health.IsDead)
            {
                return false;
            }

            Health.TakeDamage(hit.Damage);
#if UNITY_EDITOR
            Debug.Log($"{name} took {hit.Damage} damage, {Health.Current}/{Health.Max} left.", this);
#endif
            animator.SetTrigger(Health.IsDead ? KnockdownHash : GetHitHash);
            return true;
        }

        /// <summary>The Knockdown animation state ended; the dummy gets back up at full health.</summary>
        public void OnKnockdownEnded()
        {
            Health.Restore();
        }
    }
}
