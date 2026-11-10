using System.Collections.Generic;
using HollowSpirit.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;

namespace HollowSpirit.Tests.EditMode
{
    public class LockOnSelectorTests
    {
        private const float MaxDistance = 15f;
        private const float MaxAngle = 60f;
        private const float BreakDistance = 20f;

        private static readonly Vector3 Player = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 CameraPosition = new Vector3(0f, 2f, -4f);
        private static readonly Vector3 CameraForward = Vector3.forward;

        private static int Select(params Vector3[] points)
        {
            return LockOnSelector.Select(Player, CameraPosition, CameraForward, new List<Vector3>(points), MaxDistance, MaxAngle);
        }

        [Test]
        public void NothingInRange_ReturnsNone()
        {
            Assert.AreEqual(-1, Select());
            Assert.AreEqual(-1, Select(new Vector3(0f, 1f, 30f)));
        }

        [Test]
        public void TargetBehindCamera_IsNotPicked()
        {
            Assert.AreEqual(-1, Select(new Vector3(0f, 1f, -9f)));
        }

        [Test]
        public void TargetOutsideViewAngle_IsNotPicked()
        {
            Assert.AreEqual(-1, Select(new Vector3(10f, 1f, -2f)));
        }

        [Test]
        public void PicksTargetNearestTheViewCentre()
        {
            int index = Select(new Vector3(6f, 1f, 6f), new Vector3(0.5f, 1f, 5f), new Vector3(-3f, 1f, 5f));
            Assert.AreEqual(1, index);
        }

        [Test]
        public void EqualAngle_BreaksTieByDistance()
        {
            int index = Select(new Vector3(0f, 2f, 12f), new Vector3(0f, 2f, 5f));
            Assert.AreEqual(1, index);
        }

        [Test]
        public void MaxDistanceIsMeasuredFromThePlayer()
        {
            Assert.AreEqual(-1, Select(new Vector3(0f, 1f, 15.5f)));
            Assert.AreEqual(0, Select(new Vector3(0f, 1f, 14.5f)));
        }

        [Test]
        public void Release_BeyondBreakDistance()
        {
            Assert.IsTrue(LockOnSelector.ShouldRelease(Player, new Vector3(0f, 1f, BreakDistance + 1f), BreakDistance));
        }

        [Test]
        public void Keep_BetweenMaxAndBreakDistance()
        {
            Vector3 point = new Vector3(0f, 0f, (MaxDistance + BreakDistance) * 0.5f);
            Assert.IsFalse(LockOnSelector.ShouldRelease(Player, point, BreakDistance));
            Assert.AreEqual(-1, Select(point));
        }

        [Test]
        public void FlatDirection_RemovesHeight()
        {
            Vector3 direction = LockOnSelector.FlatDirection(new Vector3(1f, 0f, 1f), new Vector3(1f, 1.3f, 5f));
            Assert.AreEqual(0f, direction.y, 0.0001f);
            Assert.AreEqual(1f, direction.magnitude, 0.0001f);
            Assert.AreEqual(1f, direction.z, 0.0001f);
        }

        [Test]
        public void FlatDirection_IsZeroWhenDirectlyAbove()
        {
            Assert.AreEqual(Vector3.zero, LockOnSelector.FlatDirection(Vector3.zero, new Vector3(0f, 2f, 0f)));
        }
    }
}
