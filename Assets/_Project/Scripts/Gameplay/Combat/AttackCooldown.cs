using System;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Picks how long an enemy waits between attacks, inside the range from its definition.</summary>
    public sealed class AttackCooldown
    {
        private readonly float minSeconds;
        private readonly float maxSeconds;
        private readonly Func<float> random01;

        /// <param name="random01">Returns a value in [0, 1]; injected so tests are deterministic.</param>
        public AttackCooldown(float minSeconds, float maxSeconds, Func<float> random01)
        {
            this.minSeconds = minSeconds;
            this.maxSeconds = Math.Max(minSeconds, maxSeconds);
            this.random01 = random01;
        }

        public float Next()
        {
            float t = Math.Min(1f, Math.Max(0f, random01()));
            return minSeconds + (maxSeconds - minSeconds) * t;
        }
    }
}
