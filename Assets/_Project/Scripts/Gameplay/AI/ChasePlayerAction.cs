using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace HollowSpirit.Gameplay.AI
{
    /// <summary>Behavior node: chases the player until close enough to strike.</summary>
    [System.Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Chase Player",
        description: "Follows the player with the NavMeshAgent until it is inside attack range and at its stopping distance; fails if the enemy or the player is dead.",
        story: "Chase the player",
        category: "Action/Enemy",
        id: "8a41f2c6d9e84b7f93c05d1e2b6a7f21")]
    public partial class ChasePlayerAction : Action
    {
        private EnemyController enemy;

        protected override Status OnStart()
        {
            enemy = GameObject.GetComponent<EnemyController>();
            return enemy == null ? Status.Failure : Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (enemy.IsDead || enemy.PlayerIsDead)
            {
                enemy.StopMoving();
                return Status.Failure;
            }

            if (enemy.InAttackPosition())
            {
                enemy.StopMoving();
                return Status.Success;
            }

            enemy.ChasePlayer();
            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (enemy != null && !enemy.IsDead)
            {
                enemy.StopMoving();
            }
        }
    }
}
