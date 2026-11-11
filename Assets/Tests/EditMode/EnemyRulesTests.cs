using HollowSpirit.Gameplay.Combat;
using NUnit.Framework;

namespace HollowSpirit.Tests.EditMode
{
    public class EnemyRulesTests
    {
        [Test]
        public void Poise_HitDuringWindUp_Interrupts()
        {
            Assert.IsTrue(PoiseRule.Interrupts(false));
        }

        [Test]
        public void Poise_HitInsideDamageWindow_DoesNotInterrupt()
        {
            Assert.IsFalse(PoiseRule.Interrupts(true));
        }

        [TestCase(0f, 2f)]
        [TestCase(0.5f, 2.5f)]
        [TestCase(1f, 3f)]
        public void Cooldown_PicksInsideTheRange(float roll, float expected)
        {
            var cooldown = new AttackCooldown(2f, 3f, () => roll);
            Assert.AreEqual(expected, cooldown.Next(), 0.0001f);
        }

        [Test]
        public void Cooldown_ClampsABadRandomSource()
        {
            Assert.AreEqual(3f, new AttackCooldown(2f, 3f, () => 5f).Next(), 0.0001f);
            Assert.AreEqual(2f, new AttackCooldown(2f, 3f, () => -1f).Next(), 0.0001f);
        }

        [Test]
        public void Cooldown_ReadsTheSourceOnEveryPick()
        {
            float[] rolls = { 0.1f, 0.9f };
            int index = 0;
            var cooldown = new AttackCooldown(2f, 3f, () => rolls[index++]);

            Assert.AreEqual(2.1f, cooldown.Next(), 0.0001f);
            Assert.AreEqual(2.9f, cooldown.Next(), 0.0001f);
        }
    }
}
