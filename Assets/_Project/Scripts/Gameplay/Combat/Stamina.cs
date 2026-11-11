using System;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Stamina pool that regenerates after a delay without spending.</summary>
    public sealed class Stamina
    {
        private readonly float regenPerSecond;
        private readonly float regenDelaySeconds;
        private float lastSpendTime = float.NegativeInfinity;

        public Stamina(float max, float regenPerSecond, float regenDelaySeconds)
        {
            Max = max;
            Current = max;
            this.regenPerSecond = regenPerSecond;
            this.regenDelaySeconds = regenDelaySeconds;
        }

        public float Current { get; private set; }
        public float Max { get; }

        /// <summary>True while there is any stamina left; an action may drain it to zero.</summary>
        public bool CanSpend => Current > 0f;

        public event Action Changed;

        public void Spend(float cost, float time)
        {
            Current = Math.Max(0f, Current - cost);
            lastSpendTime = time;
            Changed?.Invoke();
        }

        /// <summary>Back to a full pool.</summary>
        public void Restore()
        {
            Current = Max;
            Changed?.Invoke();
        }

        public void Tick(float deltaTime, float time)
        {
            if (Current >= Max || time - lastSpendTime < regenDelaySeconds)
            {
                return;
            }

            Current = Math.Min(Max, Current + regenPerSecond * deltaTime);
            Changed?.Invoke();
        }
    }
}
