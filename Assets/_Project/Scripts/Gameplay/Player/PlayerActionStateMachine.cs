using System;
using HollowSpirit.Core.Input;
using HollowSpirit.Gameplay.Animation;
using HollowSpirit.Gameplay.Combat;
using HollowSpirit.Gameplay.Data;

namespace HollowSpirit.Gameplay.Player
{
    /// <summary>Top-level player actions.</summary>
    public enum PlayerActionState { Move, Roll, Attack, Stagger }

    /// <summary>Decides which action the player is in; a roll is committed until its animation finishes.</summary>
    public sealed class PlayerActionStateMachine
    {
        private readonly InputBuffer buffer;
        private readonly Stamina stamina;
        private readonly RollData roll;

        public PlayerActionStateMachine(InputBuffer buffer, Stamina stamina, RollData roll)
        {
            this.buffer = buffer;
            this.stamina = stamina;
            this.roll = roll;
        }

        public PlayerActionState State { get; private set; } = PlayerActionState.Move;
        public bool IsInvulnerable { get; private set; }

        public event Action RollStarted;

        /// <summary>Call every frame; starts buffered actions when the player is free to act.</summary>
        public void Update(float time)
        {
            if (State != PlayerActionState.Move)
            {
                return;
            }

            if (!buffer.TryConsume(BufferedAction.Roll, time) || !stamina.CanSpend)
            {
                return;
            }

            stamina.Spend(roll.staminaCost, time);
            State = PlayerActionState.Roll;
            RollStarted?.Invoke();
        }

        public void OpenWindow(ActionWindowKind kind)
        {
            if (kind == ActionWindowKind.Invulnerability)
            {
                IsInvulnerable = true;
            }
        }

        public void CloseWindow(ActionWindowKind kind)
        {
            if (kind == ActionWindowKind.Invulnerability)
            {
                IsInvulnerable = false;
            }
        }

        /// <summary>The action animation reported that it finished.</summary>
        public void NotifyActionFinished()
        {
            IsInvulnerable = false;
            State = PlayerActionState.Move;
        }
    }
}
