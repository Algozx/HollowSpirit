using UnityEngine;

namespace HollowSpirit.Gameplay.Animation
{
    /// <summary>Opens and closes an action window while an Animator state plays, using times from data.</summary>
    public sealed class ActionWindowBehaviour : StateMachineBehaviour
    {
        [SerializeField] private ActionWindowKind kind;
        [Tooltip("Data asset implementing IActionWindowSource (for example a roll or attack definition).")]
        [SerializeField] private ScriptableObject windowSource;

        private IActionWindowReceiver receiver;
        private IActionWindowSource source;
        private WindowPhase phase;
        private int actionToken;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            receiver = animator.GetComponent<IActionWindowReceiver>();
            source = windowSource as IActionWindowSource;
            phase = WindowPhase.Waiting;

            if (receiver == null || source == null)
            {
                Debug.LogError($"{nameof(ActionWindowBehaviour)}: missing receiver or window source on {animator.name}.", animator);
                return;
            }

            actionToken = receiver.OnActionStateEntered();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (receiver == null || source == null)
            {
                return;
            }

            WindowStep step = Advance(phase, stateInfo.normalizedTime * stateInfo.length, source);
            phase = step.Phase;
            if (step.Opened)
            {
                receiver.OnActionWindowOpened(kind, actionToken);
            }

            if (step.Closed)
            {
                receiver.OnActionWindowClosed(kind, actionToken);
            }
        }

        /// <summary>True while <paramref name="seconds"/> since state start is inside [start, end).</summary>
        public static bool IsOpen(float seconds, IActionWindowSource source)
        {
            return seconds >= source.WindowStartSeconds && seconds < source.WindowEndSeconds;
        }

        /// <summary>Moves the window forward to <paramref name="seconds"/>; a jump over the whole window opens and closes it once.</summary>
        public static WindowStep Advance(WindowPhase phase, float seconds, IActionWindowSource source)
        {
            switch (phase)
            {
                case WindowPhase.Waiting:
                    if (seconds < source.WindowStartSeconds)
                    {
                        return new WindowStep(WindowPhase.Waiting, false, false);
                    }

                    return seconds < source.WindowEndSeconds
                        ? new WindowStep(WindowPhase.Open, true, false)
                        : new WindowStep(WindowPhase.Done, true, true);
                case WindowPhase.Open:
                    return seconds < source.WindowEndSeconds
                        ? new WindowStep(WindowPhase.Open, false, false)
                        : new WindowStep(WindowPhase.Done, false, true);
                default:
                    return new WindowStep(WindowPhase.Done, false, false);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (receiver == null || source == null)
            {
                return;
            }

            if (phase == WindowPhase.Open)
            {
                receiver.OnActionWindowClosed(kind, actionToken);
            }

            phase = WindowPhase.Done;
            receiver.OnActionStateExited(actionToken);
        }
    }
}
