using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace HollowSpirit.Gameplay.AI
{
    /// <summary>Behavior node: waits until the enemy sees the player.</summary>
    [System.Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Await Player Sighted",
        description: "Keeps running until the enemy sees the player; fails if the enemy is dead.",
        story: "Wait until the player is sighted",
        category: "Action/Enemy",
        id: "5c0e1d0b7a3f4c8e9d2a6b1f4e7c3a10")]
    public partial class AwaitPlayerSightedAction : Action
    {
        private EnemyController enemy;

        protected override Status OnStart()
        {
            enemy = GameObject.GetComponent<EnemyController>();
            return enemy == null ? Status.Failure : Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (enemy.IsDead)
            {
                return Status.Failure;
            }

            return enemy.CanSeePlayer() ? Status.Success : Status.Running;
        }
    }
}
