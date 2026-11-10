using HollowSpirit.Gameplay.Combat;
using NUnit.Framework;

namespace HollowSpirit.Tests.EditMode
{
    public class HitRegistryTests
    {
        private sealed class Target : IDamageable
        {
            public bool TryTakeHit(in HitInfo hit)
            {
                return true;
            }
        }

        [Test]
        public void SameTarget_RegistersOncePerSwing()
        {
            var registry = new HitRegistry(4);
            var target = new Target();
            Assert.IsTrue(registry.TryRegister(target));
            Assert.IsFalse(registry.TryRegister(target));
        }

        [Test]
        public void SameTarget_RegistersAgainOnNextSwing()
        {
            var registry = new HitRegistry(4);
            var target = new Target();
            registry.TryRegister(target);
            registry.Reset();
            Assert.IsTrue(registry.TryRegister(target));
        }

        [Test]
        public void DifferentTargets_RegisterIndependently()
        {
            var registry = new HitRegistry(4);
            Assert.IsTrue(registry.TryRegister(new Target()));
            Assert.IsTrue(registry.TryRegister(new Target()));
        }
    }
}
