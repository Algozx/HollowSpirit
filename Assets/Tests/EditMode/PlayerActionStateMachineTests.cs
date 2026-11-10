using HollowSpirit.Core.Input;
using HollowSpirit.Gameplay.Animation;
using HollowSpirit.Gameplay.Combat;
using HollowSpirit.Gameplay.Data;
using HollowSpirit.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;

namespace HollowSpirit.Tests.EditMode
{
    public class PlayerActionStateMachineTests
    {
        private const float Window = 0.2f;

        private InputBuffer buffer;
        private Stamina stamina;
        private RollData roll;
        private PlayerActionStateMachine machine;

        [SetUp]
        public void SetUp()
        {
            buffer = new InputBuffer(Window);
            stamina = new Stamina(100f, 35f, 0.6f);
            roll = ScriptableObject.CreateInstance<RollData>();
            roll.staminaCost = 20f;
            roll.iFrameStartSeconds = 0.10f;
            roll.iFrameEndSeconds = 0.45f;
            machine = new PlayerActionStateMachine(buffer, stamina, roll);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(roll);
        }

        [Test]
        public void Roll_StartsFromMove_AndSpendsStamina()
        {
            buffer.Press(BufferedAction.Roll, 1f);
            machine.Update(1f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void Roll_RefusedAtZeroStamina()
        {
            stamina.Spend(100f, 0f);
            buffer.Press(BufferedAction.Roll, 1f);
            machine.Update(1f);
            Assert.AreEqual(PlayerActionState.Move, machine.State);
        }

        [Test]
        public void Roll_IsCommitted_UntilAnimationFinishes()
        {
            buffer.Press(BufferedAction.Roll, 1f);
            machine.Update(1f);
            buffer.Press(BufferedAction.Roll, 1.3f);
            machine.Update(1.3f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void Roll_BufferedNearEnd_StartsAfterCurrentRoll()
        {
            buffer.Press(BufferedAction.Roll, 1f);
            machine.Update(1f);

            buffer.Press(BufferedAction.Roll, 1.9f);
            machine.Update(1.9f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);

            machine.NotifyActionFinished();
            Assert.AreEqual(PlayerActionState.Move, machine.State);

            int started = 0;
            machine.RollStarted += () => started++;
            machine.Update(2f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);
            Assert.AreEqual(1, started);
            Assert.AreEqual(60f, stamina.Current, 0.001f);
        }

        [Test]
        public void Roll_PressedOutsideBufferWindow_IsDropped()
        {
            buffer.Press(BufferedAction.Roll, 1f);
            machine.Update(1f);

            buffer.Press(BufferedAction.Roll, 1.2f);
            machine.NotifyActionFinished();
            machine.Update(1.6f);
            Assert.AreEqual(PlayerActionState.Move, machine.State);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void Invulnerable_OnlyWhileWindowIsOpen()
        {
            Assert.IsFalse(machine.IsInvulnerable);
            machine.OpenWindow(ActionWindowKind.Invulnerability);
            Assert.IsTrue(machine.IsInvulnerable);
            machine.CloseWindow(ActionWindowKind.Invulnerability);
            Assert.IsFalse(machine.IsInvulnerable);
        }

        [Test]
        public void DamageWindow_DoesNotGrantInvulnerability()
        {
            machine.OpenWindow(ActionWindowKind.Damage);
            Assert.IsFalse(machine.IsInvulnerable);
        }

        [Test]
        public void ActionFinished_ClearsInvulnerability()
        {
            buffer.Press(BufferedAction.Roll, 1f);
            machine.Update(1f);
            machine.OpenWindow(ActionWindowKind.Invulnerability);
            machine.NotifyActionFinished();
            Assert.IsFalse(machine.IsInvulnerable);
        }

        [Test]
        public void RollData_ProvidesWindowTimes()
        {
            IActionWindowSource source = roll;
            Assert.AreEqual(0.10f, source.WindowStartSeconds, 0.0001f);
            Assert.AreEqual(0.45f, source.WindowEndSeconds, 0.0001f);
        }

        [TestCase(0.09f, false)]
        [TestCase(0.10f, true)]
        [TestCase(0.449f, true)]
        [TestCase(0.45f, false)]
        public void RollWindow_OpenOnlyBetweenStartAndEnd(float seconds, bool expected)
        {
            roll.iFrameStartSeconds = 0.10f;
            roll.iFrameEndSeconds = 0.45f;
            Assert.AreEqual(expected, ActionWindowBehaviour.IsOpen(seconds, roll));
        }
    }
}
