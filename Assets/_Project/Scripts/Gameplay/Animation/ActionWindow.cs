namespace HollowSpirit.Gameplay.Animation
{
    /// <summary>Kinds of timed windows an action animation can open.</summary>
    public enum ActionWindowKind { Invulnerability, Damage }

    /// <summary>Receives action window events from the Animator state behaviours.</summary>
    public interface IActionWindowReceiver
    {
        void OnActionWindowOpened(ActionWindowKind kind);
        void OnActionWindowClosed(ActionWindowKind kind);
        void OnActionStateExited();
    }

    /// <summary>Data assets that provide window times implement this.</summary>
    public interface IActionWindowSource
    {
        float WindowStartSeconds { get; }
        float WindowEndSeconds { get; }
    }
}
