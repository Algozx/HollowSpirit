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
        private AttackData attack1;
        private AttackData attack2;
        private Health health;
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
            attack1 = MakeAttack(15f, 20f, 0.2f, 0.4f);
            attack2 = MakeAttack(15f, 20f, 0.25f, 0.45f);
            health = new Health(100f);
            machine = new PlayerActionStateMachine(buffer, stamina, roll, new[] { attack1, attack2 }, health);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(roll);
            Object.DestroyImmediate(attack1);
            Object.DestroyImmediate(attack2);
        }

        private static AttackData MakeAttack(float cost, float damage, float start, float end)
        {
            AttackData data = ScriptableObject.CreateInstance<AttackData>();
            data.staminaCost = cost;
            data.damage = damage;
            data.damageStartSeconds = start;
            data.damageEndSeconds = end;
            return data;
        }

        private void PressAndUpdate(BufferedAction action, float time)
        {
            buffer.Press(action, time);
            machine.Update(time);
        }

        [Test]
        public void Roll_StartsFromMove_AndSpendsStamina()
        {
            PressAndUpdate(BufferedAction.Roll, 1f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void Roll_RefusedAtZeroStamina()
        {
            stamina.Spend(100f, 0f);
            PressAndUpdate(BufferedAction.Roll, 1f);
            Assert.AreEqual(PlayerActionState.Move, machine.State);
        }

        [Test]
        public void Roll_IsCommitted_UntilAnimationFinishes()
        {
            PressAndUpdate(BufferedAction.Roll, 1f);
            PressAndUpdate(BufferedAction.Roll, 1.3f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void Roll_BufferedNearEnd_StartsAfterCurrentRoll()
        {
            PressAndUpdate(BufferedAction.Roll, 1f);
            int rollToken = machine.BeginAction();

            PressAndUpdate(BufferedAction.Roll, 1.9f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);

            machine.NotifyActionFinished(rollToken);
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
            PressAndUpdate(BufferedAction.Roll, 1f);
            int rollToken = machine.BeginAction();

            buffer.Press(BufferedAction.Roll, 1.2f);
            machine.NotifyActionFinished(rollToken);
            machine.Update(1.6f);
            Assert.AreEqual(PlayerActionState.Move, machine.State);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void Invulnerable_OnlyWhileWindowIsOpen()
        {
            Assert.IsFalse(machine.IsInvulnerable);
            int token = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Invulnerability, token);
            Assert.IsTrue(machine.IsInvulnerable);
            machine.CloseWindow(ActionWindowKind.Invulnerability, token);
            Assert.IsFalse(machine.IsInvulnerable);
        }

        [Test]
        public void DamageWindow_DoesNotGrantInvulnerability()
        {
            machine.OpenWindow(ActionWindowKind.Damage, machine.BeginAction());
            Assert.IsFalse(machine.IsInvulnerable);
        }

        [Test]
        public void ActionFinished_ClearsInvulnerability()
        {
            PressAndUpdate(BufferedAction.Roll, 1f);
            int token = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Invulnerability, token);
            machine.NotifyActionFinished(token);
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

        [Test]
        public void Attack_StartsFromMove_AndSpendsStamina()
        {
            PressAndUpdate(BufferedAction.Attack, 1f);
            Assert.AreEqual(PlayerActionState.Attack, machine.State);
            Assert.AreSame(attack1, machine.CurrentAttack);
            Assert.AreEqual(85f, stamina.Current, 0.001f);
        }

        [Test]
        public void Attack_RefusedAtZeroStamina()
        {
            stamina.Spend(100f, 0f);
            PressAndUpdate(BufferedAction.Attack, 1f);
            Assert.AreEqual(PlayerActionState.Move, machine.State);
            Assert.IsNull(machine.CurrentAttack);
        }

        [Test]
        public void Attack_BufferedAfterDamageWindow_ChainsOneTwoOne()
        {
            PressAndUpdate(BufferedAction.Attack, 1f);
            int token1 = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Damage, token1);
            machine.CloseWindow(ActionWindowKind.Damage, token1);

            PressAndUpdate(BufferedAction.Attack, 1.4f);
            Assert.AreSame(attack2, machine.CurrentAttack);
            int token2 = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Damage, token2);
            machine.CloseWindow(ActionWindowKind.Damage, token2);

            PressAndUpdate(BufferedAction.Attack, 1.9f);
            Assert.AreSame(attack1, machine.CurrentAttack);
            Assert.AreEqual(55f, stamina.Current, 0.001f);
        }

        [Test]
        public void Attack_ChainResets_AfterReturningToMove()
        {
            PressAndUpdate(BufferedAction.Attack, 1f);
            int token1 = machine.BeginAction();
            machine.CloseWindow(ActionWindowKind.Damage, token1);
            machine.NotifyActionFinished(token1);
            Assert.AreEqual(PlayerActionState.Move, machine.State);

            PressAndUpdate(BufferedAction.Attack, 3f);
            Assert.AreSame(attack1, machine.CurrentAttack);
        }

        [Test]
        public void Attack_BufferedBeforeDamageWindowEnds_WaitsInBuffer()
        {
            PressAndUpdate(BufferedAction.Attack, 1f);
            int token1 = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Damage, token1);

            PressAndUpdate(BufferedAction.Attack, 1.2f);
            Assert.AreSame(attack1, machine.CurrentAttack);
            Assert.AreEqual(85f, stamina.Current, 0.001f);

            machine.CloseWindow(ActionWindowKind.Damage, token1);
            machine.Update(1.3f);
            Assert.AreSame(attack2, machine.CurrentAttack);
            Assert.AreEqual(70f, stamina.Current, 0.001f);
        }

        [Test]
        public void Roll_BufferedBeforeDamageWindowEnds_WaitsThenCancelsRecovery()
        {
            PressAndUpdate(BufferedAction.Attack, 1f);
            int token1 = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Damage, token1);

            int started = 0;
            machine.RollStarted += () => started++;
            PressAndUpdate(BufferedAction.Roll, 1.2f);
            Assert.AreEqual(PlayerActionState.Attack, machine.State);
            Assert.AreEqual(0, started);

            machine.CloseWindow(ActionWindowKind.Damage, token1);
            machine.Update(1.3f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);
            Assert.AreEqual(1, started);
            Assert.AreEqual(65f, stamina.Current, 0.001f);
        }

        [Test]
        public void Attack_DoesNotCancelARollInProgress()
        {
            PressAndUpdate(BufferedAction.Roll, 1f);
            int token = machine.BeginAction();
            machine.CloseWindow(ActionWindowKind.Damage, token);
            PressAndUpdate(BufferedAction.Attack, 1.1f);
            Assert.AreEqual(PlayerActionState.Roll, machine.State);
            Assert.AreEqual(80f, stamina.Current, 0.001f);
        }

        [Test]
        public void StaleExit_FromPreviousAttack_DoesNotEndTheCurrentOne()
        {
            PressAndUpdate(BufferedAction.Attack, 1f);
            int token1 = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Damage, token1);
            machine.CloseWindow(ActionWindowKind.Damage, token1);

            PressAndUpdate(BufferedAction.Attack, 1.4f);
            int token2 = machine.BeginAction();

            machine.NotifyActionFinished(token1);
            Assert.AreEqual(PlayerActionState.Attack, machine.State);
            Assert.AreSame(attack2, machine.CurrentAttack);

            machine.NotifyActionFinished(token2);
            Assert.AreEqual(PlayerActionState.Move, machine.State);
        }

        [Test]
        public void ExitOfPreviousState_BeforeNewStateEnters_DoesNotCancelTheNewAction()
        {
            PressAndUpdate(BufferedAction.Attack, 1f);
            int token1 = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Damage, token1);
            machine.CloseWindow(ActionWindowKind.Damage, token1);

            PressAndUpdate(BufferedAction.Attack, 1.4f);
            machine.NotifyActionFinished(token1);

            Assert.AreEqual(PlayerActionState.Attack, machine.State);
            Assert.AreSame(attack2, machine.CurrentAttack);
        }

        [TestCase(0.1f, 0.17f, 1)]
        [TestCase(0.17f, 0.17f, 1)]
        [TestCase(0.5f, 0.17f, 3)]
        [TestCase(1.9f, 0.17f, 12)]
        [TestCase(1f, 0f, 1)]
        public void HitboxSubsteps_KeepPosesWithinOneRadius(float displacement, float radius, int expected)
        {
            Assert.AreEqual(expected, HollowSpirit.Gameplay.Combat.WeaponHitbox.SubstepCount(displacement, radius, 12));
        }

        [Test]
        public void StaleWindowCalls_AreIgnored()
        {
            int oldToken = machine.BeginAction();
            int newToken = machine.BeginAction();
            Assert.IsFalse(machine.OpenWindow(ActionWindowKind.Invulnerability, oldToken));
            Assert.IsFalse(machine.IsInvulnerable);
            Assert.IsTrue(machine.OpenWindow(ActionWindowKind.Invulnerability, newToken));
            Assert.IsFalse(machine.CloseWindow(ActionWindowKind.Invulnerability, oldToken));
            Assert.IsTrue(machine.IsInvulnerable);
        }

        [Test]
        public void Player_IgnoresHits_WhileInvulnerable()
        {
            int token = machine.BeginAction();
            machine.OpenWindow(ActionWindowKind.Invulnerability, token);
            var hit = new HitInfo(30f, null, Vector3.zero);
            Assert.IsFalse(machine.TryTakeHit(hit));
            Assert.AreEqual(100f, health.Current, 0.001f);
        }

        [Test]
        public void Player_TakesHits_WhenNotInvulnerable()
        {
            var hit = new HitInfo(30f, null, Vector3.zero);
            Assert.IsTrue(machine.TryTakeHit(hit));
            Assert.AreEqual(70f, health.Current, 0.001f);
        }

        [Test]
        public void Player_IgnoresHits_WhenDead()
        {
            health.TakeDamage(100f);
            Assert.IsFalse(machine.TryTakeHit(new HitInfo(10f, null, Vector3.zero)));
        }

        [Test]
        public void AttackData_ProvidesWindowTimes()
        {
            IActionWindowSource source = attack1;
            Assert.AreEqual(0.2f, source.WindowStartSeconds, 0.0001f);
            Assert.AreEqual(0.4f, source.WindowEndSeconds, 0.0001f);
        }

        [Test]
        public void Advance_NormalEdges_OpenThenCloseOnce()
        {
            WindowStep waiting = ActionWindowBehaviour.Advance(WindowPhase.Waiting, 0.1f, attack1);
            Assert.AreEqual(WindowPhase.Waiting, waiting.Phase);
            Assert.IsFalse(waiting.Opened);
            Assert.IsFalse(waiting.Closed);

            WindowStep opened = ActionWindowBehaviour.Advance(waiting.Phase, 0.2f, attack1);
            Assert.AreEqual(WindowPhase.Open, opened.Phase);
            Assert.IsTrue(opened.Opened);
            Assert.IsFalse(opened.Closed);

            WindowStep still = ActionWindowBehaviour.Advance(opened.Phase, 0.3f, attack1);
            Assert.IsFalse(still.Opened);
            Assert.IsFalse(still.Closed);

            WindowStep closed = ActionWindowBehaviour.Advance(still.Phase, 0.4f, attack1);
            Assert.AreEqual(WindowPhase.Done, closed.Phase);
            Assert.IsFalse(closed.Opened);
            Assert.IsTrue(closed.Closed);

            WindowStep after = ActionWindowBehaviour.Advance(closed.Phase, 0.5f, attack1);
            Assert.IsFalse(after.Opened);
            Assert.IsFalse(after.Closed);
        }

        [Test]
        public void Advance_FrameJumpingOverWindow_OpensAndClosesOnceInThatUpdate()
        {
            WindowStep jump = ActionWindowBehaviour.Advance(WindowPhase.Waiting, 0.6f, attack1);
            Assert.IsTrue(jump.Opened);
            Assert.IsTrue(jump.Closed);
            Assert.AreEqual(WindowPhase.Done, jump.Phase);

            WindowStep next = ActionWindowBehaviour.Advance(jump.Phase, 0.7f, attack1);
            Assert.IsFalse(next.Opened);
            Assert.IsFalse(next.Closed);
        }
    }
}
