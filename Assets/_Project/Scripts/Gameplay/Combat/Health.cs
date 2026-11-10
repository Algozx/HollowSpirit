using System;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Hit points of a character; raises Changed on every change and Died once.</summary>
    public sealed class Health
    {
        public Health(float max)
        {
            Max = max;
            Current = max;
        }

        public float Current { get; private set; }
        public float Max { get; }
        public bool IsDead => Current <= 0f;

        public event Action Changed;
        public event Action Died;

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            Current = Math.Max(0f, Current - amount);
            Changed?.Invoke();
            if (IsDead)
            {
                Died?.Invoke();
            }
        }

        /// <summary>Back to full health; Died can fire again afterwards.</summary>
        public void Restore()
        {
            Current = Max;
            Changed?.Invoke();
        }
    }
}
