using UnityEngine;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>What a hit carries: damage, who dealt it and where it landed.</summary>
    public readonly struct HitInfo
    {
        public readonly float Damage;
        public readonly GameObject Attacker;
        public readonly Vector3 Point;

        public HitInfo(float damage, GameObject attacker, Vector3 point)
        {
            Damage = damage;
            Attacker = attacker;
            Point = point;
        }
    }

    /// <summary>Something that can be hit.</summary>
    public interface IDamageable
    {
        /// <summary>Returns false when the hit is ignored (dead or invulnerable).</summary>
        bool TryTakeHit(in HitInfo hit);
    }
}
