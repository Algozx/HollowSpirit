using HollowSpirit.Gameplay.Combat;
using NUnit.Framework;

namespace HollowSpirit.Tests.EditMode
{
    public class StaminaTests
    {
        private const float Max = 100f;
        private const float Regen = 35f;
        private const float Delay = 0.6f;

        private static Stamina Create() => new Stamina(Max, Regen, Delay);

        [Test]
        public void Spend_ReducesCurrent()
        {
            var stamina = Create();
            stamina.Spend(20f, 0f);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void CannotSpend_AtZero()
        {
            var stamina = Create();
            stamina.Spend(Max, 0f);
            Assert.IsFalse(stamina.CanSpend);
        }

        [Test]
        public void Spend_MayTakeStaminaToZeroButNotBelow()
        {
            var stamina = Create();
            stamina.Spend(95f, 0f);
            Assert.IsTrue(stamina.CanSpend);
            stamina.Spend(20f, 0.1f);
            Assert.AreEqual(0f, stamina.Current, 0.001f);
        }

        [Test]
        public void NoRegen_BeforeDelay()
        {
            var stamina = Create();
            stamina.Spend(50f, 1f);
            stamina.Tick(0.3f, 1.3f);
            Assert.AreEqual(50f, stamina.Current, 0.001f);
        }

        [Test]
        public void Regen_AfterDelay()
        {
            var stamina = Create();
            stamina.Spend(50f, 1f);
            stamina.Tick(1f, 1.7f);
            Assert.AreEqual(85f, stamina.Current, 0.001f);
        }

        [Test]
        public void Spend_RestartsRegenDelay()
        {
            var stamina = Create();
            stamina.Spend(50f, 1f);
            stamina.Spend(10f, 2f);
            stamina.Tick(0.1f, 2.3f);
            Assert.AreEqual(40f, stamina.Current, 0.001f);
        }

        [Test]
        public void Regen_ClampsToMax()
        {
            var stamina = Create();
            stamina.Spend(10f, 0f);
            stamina.Tick(5f, 5f);
            Assert.AreEqual(Max, stamina.Current, 0.001f);
        }

        [Test]
        public void Changed_RaisedOnSpend()
        {
            var stamina = Create();
            int raised = 0;
            stamina.Changed += () => raised++;
            stamina.Spend(10f, 0f);
            Assert.AreEqual(1, raised);
        }
    }
}
