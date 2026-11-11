using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace HollowSpirit.Gameplay.AI
{
    /// <summary>Behavior node: waits a random time between the enemy's attack cooldown limits.</summary>
    [System.Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Attack Cooldown",
        description: "Waits a random time between the minimum and maximum attack cooldown of the enemy.",
        story: "Wait for the attack cooldown",
        category: "Action/Enemy",
        id: "e9f1a3c5b7d94e60a2b4c6d8f0e1a3b4")]
    public partial class AttackCooldownAction : Action
    {
        [CreateProperty] private float remainingSeconds;
        private EnemyController enemy;

        protected override Status OnStart()
        {
            enemy = GameObject.GetComponent<EnemyController>();
            if (enemy == null)
            {
                return Status.Failure;
            }

            remainingSeconds = enemy.NextCooldownSeconds();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            remainingSeconds -= Time.deltaTime;
            return remainingSeconds <= 0f ? Status.Success : Status.Running;
        }
    }
}
