using UnityEngine;

namespace HollowSpirit.Gameplay.Animation
{
    /// <summary>Marks an Animator state as a committed action without any window; its exit ends the action.</summary>
    public sealed class ActionStateBehaviour : StateMachineBehaviour
    {
        private IActionWindowReceiver receiver;
        private int actionToken;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            receiver = animator.GetComponent<IActionWindowReceiver>();
            if (receiver == null)
            {
                Debug.LogError($"{nameof(ActionStateBehaviour)}: no receiver on {animator.name}.", animator);
                return;
            }

            actionToken = receiver.OnActionStateEntered();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            receiver?.OnActionStateExited(actionToken);
        }
    }
}
