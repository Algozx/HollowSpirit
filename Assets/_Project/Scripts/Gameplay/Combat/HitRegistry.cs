using System.Collections.Generic;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Remembers who was already hit during the current swing so each target is hit once.</summary>
    public sealed class HitRegistry
    {
        private readonly List<IDamageable> hitTargets;

        public HitRegistry(int capacity)
        {
            hitTargets = new List<IDamageable>(capacity);
        }

        /// <summary>True the first time a target is offered since the last Reset.</summary>
        public bool TryRegister(IDamageable target)
        {
            if (hitTargets.Contains(target))
            {
                return false;
            }

            hitTargets.Add(target);
            return true;
        }

        /// <summary>Starts a new swing.</summary>
        public void Reset()
        {
            hitTargets.Clear();
        }
    }
}
