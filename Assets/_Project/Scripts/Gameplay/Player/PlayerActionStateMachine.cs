using System;
using System.Collections.Generic;
using HollowSpirit.Core.Input;
using HollowSpirit.Gameplay.Animation;
using HollowSpirit.Gameplay.Combat;
using HollowSpirit.Gameplay.Data;

namespace HollowSpirit.Gameplay.Player
{
    /// <summary>Top-level player actions.</summary>
    public enum PlayerActionState { Move, Roll, Attack, Stagger, Dead }

    /// <summary>Decides which action the player is in; rolls, attacks and staggers are committed until their animation ends or is cancelled.</summary>
    public sealed class PlayerActionStateMachine : IDamageable
    {
        private readonly InputBuffer buffer;
        private readonly Stamina stamina;
        private readonly RollData roll;
        private readonly IReadOnlyList<AttackData> lightChain;
        private readonly Health health;
        private int nextAttackIndex;
        private bool damageWindowEnded;
        private int tokenCounter;
        private int currentToken;

        public PlayerActionStateMachine(InputBuffer buffer, Stamina stamina, RollData roll, IReadOnlyList<AttackData> lightChain, Health health)
        {
            this.buffer = buffer;
            this.stamina = stamina;
            this.roll = roll;
            this.lightChain = lightChain;
            this.health = health;
        }

        public PlayerActionState State { get; private set; } = PlayerActionState.Move;
        public bool IsInvulnerable { get; private set; }
        public AttackData CurrentAttack { get; private set; }

        public event Action RollStarted;
        public event Action<AttackData, int> AttackStarted;
        public event Action StaggerStarted;
        public event Action Died;

        /// <summary>Call every frame; starts buffered actions when the player is free to act.</summary>
        public void Update(float time)
        {
            bool canCancel = State == PlayerActionState.Attack && damageWindowEnded;
            if (State != PlayerActionState.Move && !canCancel)
            {
                return;
            }

            if (buffer.TryConsume(BufferedAction.Roll, time))
            {
                TryStartRoll(time);
            }
            else if (buffer.TryConsume(BufferedAction.Attack, time))
            {
                TryStartAttack(time);
            }
        }

        /// <summary>The player is hit; ignored while dead or invulnerable. A hit interrupts the current action.</summary>
        public bool TryTakeHit(in HitInfo hit)
        {
            if (State == PlayerActionState.Dead || health.IsDead || IsInvulnerable)
            {
                return false;
            }

            health.TakeDamage(hit.Damage);
            buffer.Clear();
            if (health.IsDead)
            {
                InterruptCurrentAction();
                State = PlayerActionState.Dead;
                Died?.Invoke();
                return true;
            }

            // A hit while already staggered only deals damage; the flinch is committed until it ends.
            if (State != PlayerActionState.Stagger)
            {
                InterruptCurrentAction();
                State = PlayerActionState.Stagger;
                StaggerStarted?.Invoke();
            }

            return true;
        }

        /// <summary>Back on their feet with full health and stamina, ready to act.</summary>
        public void Respawn()
        {
            health.Restore();
            stamina.Restore();
            buffer.Clear();
            InterruptCurrentAction();
            State = PlayerActionState.Move;
        }

        /// <summary>An action animation state started; returns the token its later calls must carry.</summary>
        public int BeginAction()
        {
            tokenCounter++;
            currentToken = tokenCounter;
            return currentToken;
        }

        /// <summary>Returns false when the call came from an older action and was ignored.</summary>
        public bool OpenWindow(ActionWindowKind kind, int actionToken)
        {
            if (actionToken != currentToken)
            {
                return false;
            }

            if (kind == ActionWindowKind.Invulnerability)
            {
                IsInvulnerable = true;
            }

            return true;
        }

        /// <summary>Returns false when the call came from an older action and was ignored.</summary>
        public bool CloseWindow(ActionWindowKind kind, int actionToken)
        {
            if (actionToken != currentToken)
            {
                return false;
            }

            if (kind == ActionWindowKind.Invulnerability)
            {
                IsInvulnerable = false;
            }
            else
            {
                damageWindowEnded = true;
            }

            return true;
        }

        /// <summary>The action animation reported that it finished; calls from an older action are ignored.</summary>
        public void NotifyActionFinished(int actionToken)
        {
            if (actionToken != currentToken || State == PlayerActionState.Dead)
            {
                return;
            }

            currentToken = 0;
            IsInvulnerable = false;
            State = PlayerActionState.Move;
            CurrentAttack = null;
            nextAttackIndex = 0;
        }

        private void TryStartRoll(float time)
        {
            if (!stamina.CanSpend)
            {
                return;
            }

            stamina.Spend(roll.staminaCost, time);
            InvalidateCurrentAction();
            State = PlayerActionState.Roll;
            CurrentAttack = null;
            RollStarted?.Invoke();
        }

        private void TryStartAttack(float time)
        {
            if (!stamina.CanSpend || lightChain.Count == 0)
            {
                return;
            }

            int index = nextAttackIndex;
            CurrentAttack = lightChain[index];
            nextAttackIndex = (index + 1) % lightChain.Count;
            damageWindowEnded = false;
            stamina.Spend(CurrentAttack.staminaCost, time);
            InvalidateCurrentAction();
            State = PlayerActionState.Attack;
            AttackStarted?.Invoke(CurrentAttack, index);
        }

        /// <summary>Calls from the state being left are stale until the new action's state enters.</summary>
        private void InvalidateCurrentAction()
        {
            currentToken = 0;
            IsInvulnerable = false;
        }

        private void InterruptCurrentAction()
        {
            InvalidateCurrentAction();
            CurrentAttack = null;
            nextAttackIndex = 0;
            damageWindowEnded = false;
        }
    }
}
