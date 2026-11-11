using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace HollowSpirit.Gameplay.AI
{
    /// <summary>Behavior node: faces the player and plays the attack until it ends.</summary>
    [System.Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Attack Player",
        description: "Faces the player and swings; succeeds when the attack animation has ended or was interrupted.",
        story: "Attack the player",
        category: "Action/Enemy",
        id: "c3d7b5a190ef4a28b6e4f81d0a9c5e32")]
    public partial class AttackPlayerAction : Action
    {
        private EnemyController enemy;

        protected override Status OnStart()
        {
            enemy = GameObject.GetComponent<EnemyController>();
            if (enemy == null || !enemy.TryBeginAttack())
            {
                return Status.Failure;
            }

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            return enemy.IsDead || !enemy.IsAttackBusy ? Status.Success : Status.Running;
        }
    }
}
