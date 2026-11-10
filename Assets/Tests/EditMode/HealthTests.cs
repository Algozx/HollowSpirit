using HollowSpirit.Gameplay.Combat;
using NUnit.Framework;

namespace HollowSpirit.Tests.EditMode
{
    public class HealthTests
    {
        [Test]
        public void TakeDamage_ReducesCurrent_AndRaisesChanged()
        {
            var health = new Health(100f);
            int changed = 0;
            health.Changed += () => changed++;
            health.TakeDamage(20f);
            Assert.AreEqual(80f, health.Current, 0.001f);
            Assert.AreEqual(1, changed);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void TakeDamage_ClampsAtZero()
        {
            var health = new Health(50f);
            health.TakeDamage(80f);
            Assert.AreEqual(0f, health.Current, 0.001f);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void Died_FiresOnce()
        {
            var health = new Health(30f);
            int died = 0;
            health.Died += () => died++;
            health.TakeDamage(30f);
            health.TakeDamage(10f);
            Assert.AreEqual(1, died);
        }

        [Test]
        public void Restore_BringsBackFullHealth_AndAllowsDyingAgain()
        {
            var health = new Health(30f);
            int died = 0;
            health.Died += () => died++;
            health.TakeDamage(30f);
            health.Restore();
            Assert.AreEqual(30f, health.Current, 0.001f);
            Assert.IsFalse(health.IsDead);
            health.TakeDamage(30f);
            Assert.AreEqual(2, died);
        }
    }
}
