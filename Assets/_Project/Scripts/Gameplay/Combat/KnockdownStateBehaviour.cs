using UnityEngine;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Tells the training dummy when its Knockdown animation state has ended.</summary>
    public sealed class KnockdownStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            TrainingDummy dummy = animator.GetComponent<TrainingDummy>();
            if (dummy != null)
            {
                dummy.OnKnockdownEnded();
            }
        }
    }
}
