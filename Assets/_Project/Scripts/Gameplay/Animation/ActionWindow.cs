namespace HollowSpirit.Gameplay.Animation
{
    /// <summary>Kinds of timed windows an action animation can open.</summary>
    public enum ActionWindowKind { Invulnerability, Damage }

    /// <summary>Receives action window events from the Animator state behaviours; calls carry the token issued on state enter.</summary>
    public interface IActionWindowReceiver
    {
        /// <summary>An action state was entered; returns the token every later call of that state must carry.</summary>
        int OnActionStateEntered();
        void OnActionWindowOpened(ActionWindowKind kind, int actionToken);
        void OnActionWindowClosed(ActionWindowKind kind, int actionToken);
        void OnActionStateExited(int actionToken);
    }

    /// <summary>Data assets that provide window times implement this.</summary>
    public interface IActionWindowSource
    {
        float WindowStartSeconds { get; }
        float WindowEndSeconds { get; }
    }

    /// <summary>Where a window is in its life: not reached yet, open, or already closed.</summary>
    public enum WindowPhase { Waiting, Open, Done }

    /// <summary>What one update did to a window; a skipped window reports both flags.</summary>
    public readonly struct WindowStep
    {
        public readonly WindowPhase Phase;
        public readonly bool Opened;
        public readonly bool Closed;

        public WindowStep(WindowPhase phase, bool opened, bool closed)
        {
            Phase = phase;
            Opened = opened;
            Closed = closed;
        }
    }
}
