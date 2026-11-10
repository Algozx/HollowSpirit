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
        private bool windowOpen;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            receiver = animator.GetComponent<IActionWindowReceiver>();
            source = windowSource as IActionWindowSource;
            windowOpen = false;

            if (receiver == null || source == null)
            {
                Debug.LogError($"{nameof(ActionWindowBehaviour)}: missing receiver or window source on {animator.name}.", animator);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (receiver == null || source == null)
            {
                return;
            }

            bool shouldBeOpen = IsOpen(stateInfo.normalizedTime * stateInfo.length, source);
            if (shouldBeOpen == windowOpen)
            {
                return;
            }

            windowOpen = shouldBeOpen;
            if (windowOpen)
            {
                receiver.OnActionWindowOpened(kind);
            }
            else
            {
                receiver.OnActionWindowClosed(kind);
            }
        }

        /// <summary>True while <paramref name="seconds"/> since state start is inside [start, end).</summary>
        public static bool IsOpen(float seconds, IActionWindowSource source)
        {
            return seconds >= source.WindowStartSeconds && seconds < source.WindowEndSeconds;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (receiver == null)
            {
                return;
            }

            if (windowOpen)
            {
                windowOpen = false;
                receiver.OnActionWindowClosed(kind);
            }

            receiver.OnActionStateExited();
        }
    }
}
