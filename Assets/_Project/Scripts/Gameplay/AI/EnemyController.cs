using System;
using HollowSpirit.Gameplay.Animation;
using HollowSpirit.Gameplay.Combat;
using HollowSpirit.Gameplay.Data;
using HollowSpirit.Gameplay.Player;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace HollowSpirit.Gameplay.AI
{
    /// <summary>Body of a melee enemy: owns its health, poise and weapon, and offers the senses and moves the Behavior graph nodes use.</summary>
    [RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
    public sealed class EnemyController : MonoBehaviour, IDamageable, IActionWindowReceiver
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int HitStateHash = Animator.StringToHash("Hit");

        // Safety net: if the Animator never reaches the Hit state, the enemy must not stay frozen.
        private const float StaggerStartTimeoutSeconds = 0.5f;

        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private WeaponHitbox weaponHitbox;
        [Tooltip("The player this enemy hunts; wired per instance in the scene.")]
        [SerializeField] private PlayerController player;
        [Tooltip("Where sight lines start.")]
        [SerializeField] private Transform eyePoint;
        [Tooltip("Layers that block the line of sight.")]
        [SerializeField] private LayerMask sightBlockers;
        [Tooltip("Seconds the locomotion blend takes to follow the agent speed.")]
        [SerializeField] private float speedDampSeconds;
        [Tooltip("Colliders switched off when the enemy dies: hurtbox and body.")]
        [SerializeField] private Collider[] collidersOffOnDeath;
        [SerializeField] private LockOnTarget lockOnTarget;

        private Animator animator;
        private NavMeshAgent agent;
        private BehaviorGraphAgent graphAgent;
        private AttackCooldown cooldown;
        private int attackToken;
        private int tokenCounter;
        private bool attackRequested;
        private bool inAttackState;
        private bool damageWindowOpen;
        private bool staggered;
        private bool staggerSeen;
        private float staggerStartedTime;

        public Health Health { get; private set; }
        public bool IsDead => Health == null || Health.IsDead;
        public bool IsAttackBusy => attackRequested || inAttackState;
        public bool IsStaggered => staggered;
        public bool PlayerIsDead => player == null || player.IsDead;

        /// <summary>Raised once on death with the souls the enemy drops.</summary>
        public event Action<int> Died;

        private void Awake()
        {
            if (definition == null || weaponHitbox == null || player == null || eyePoint == null
                || definition.attacks == null || definition.attacks.Length == 0 || definition.attacks[0] == null)
            {
                Debug.LogError($"{nameof(EnemyController)}: definition, weapon hitbox, player, eye point or attack is missing on {name}.", this);
                enabled = false;
                return;
            }

            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            graphAgent = GetComponent<BehaviorGraphAgent>();
            Health = new Health(definition.maxHealth);
            agent.stoppingDistance = Mathf.Max(0f, definition.attackRange - definition.arrivalToleranceMetres);
            cooldown = new AttackCooldown(definition.attackCooldownMinSeconds, definition.attackCooldownMaxSeconds, UnityRandom01);
        }

        private static float UnityRandom01()
        {
            return UnityEngine.Random.value;
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            UpdateStagger();
            float speed = agent.isStopped || agent.speed <= 0f ? 0f : agent.velocity.magnitude / agent.speed;
            animator.SetFloat(SpeedHash, speed, speedDampSeconds, Time.deltaTime);
        }

        // The Hit trigger is consumed on the next animator update, so wait until the state was seen before calling it over.
        private void UpdateStagger()
        {
            if (!staggered)
            {
                return;
            }

            bool inHit = animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HitStateHash
                || (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == HitStateHash);
            if (inHit)
            {
                staggerSeen = true;
            }
            else if (staggerSeen || Time.time - staggerStartedTime > StaggerStartTimeoutSeconds)
            {
                staggered = false;
                staggerSeen = false;
            }
        }

        // Senses and moves used by the Behavior nodes.

        public float FlatDistanceToPlayer()
        {
            Vector3 offset = player.transform.position - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        /// <summary>True when the player is alive, inside sight range and not behind an obstacle.</summary>
        public bool CanSeePlayer()
        {
            if (player.IsDead || FlatDistanceToPlayer() > definition.sightRange)
            {
                return false;
            }

            return !Physics.Linecast(eyePoint.position, player.CenterPoint, sightBlockers, QueryTriggerInteraction.Ignore);
        }

        public bool PlayerInAttackRange()
        {
            return FlatDistanceToPlayer() <= definition.attackRange;
        }

        /// <summary>True once the player is inside attack range, which the blade can reach.</summary>
        public bool InAttackPosition()
        {
            return PlayerInAttackRange();
        }

        public void ChasePlayer()
        {
            if (IsDead || staggered)
            {
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }

        public void StopMoving()
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        public float NextCooldownSeconds()
        {
            return cooldown.Next();
        }

        /// <summary>Turns toward the player and starts the swing; false when the enemy cannot attack right now.</summary>
        public bool TryBeginAttack()
        {
            if (IsDead || staggered || IsAttackBusy)
            {
                return false;
            }

            StopMoving();
            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up) * Quaternion.Euler(0f, -definition.attackAimOffsetDegrees, 0f);
            }

            attackRequested = true;
            animator.SetTrigger(AttackHash);
            return true;
        }

        // Damage.

        public bool TryTakeHit(in HitInfo hit)
        {
            // Enemies do not hurt each other.
            if (IsDead || (hit.Attacker != null && hit.Attacker.TryGetComponent(out EnemyController _)))
            {
                return false;
            }

            bool interrupts = PoiseRule.Interrupts(damageWindowOpen);
            Health.TakeDamage(hit.Damage);
            if (Health.IsDead)
            {
                Die();
                return true;
            }

            if (interrupts && !staggered)
            {
                Stagger();
            }

            return true;
        }

        private void Stagger()
        {
            weaponHitbox.EndSwing();
            StopMoving();
            attackRequested = false;

            // Window calls still arriving from the attack state during the blend are stale.
            attackToken = 0;
            inAttackState = false;
            damageWindowOpen = false;

            animator.ResetTrigger(AttackHash);
            staggered = true;
            staggerSeen = false;
            staggerStartedTime = Time.time;
            animator.SetTrigger(HitHash);
        }

        private void Die()
        {
            weaponHitbox.EndSwing();
            weaponHitbox.enabled = false;
            attackRequested = false;
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(HitHash);
            animator.SetFloat(SpeedHash, 0f);

            if (graphAgent != null)
            {
                graphAgent.End();
                graphAgent.enabled = false;
            }

            StopMoving();
            agent.enabled = false;
            for (int i = 0; i < collidersOffOnDeath.Length; i++)
            {
                collidersOffOnDeath[i].enabled = false;
            }

            if (lockOnTarget != null)
            {
                lockOnTarget.enabled = false;
            }

            animator.SetTrigger(DieHash);
            Died?.Invoke(definition.souls);
        }

        // Action window events from the Animator attack state.

        public int OnActionStateEntered()
        {
            tokenCounter++;
            attackToken = tokenCounter;
            inAttackState = true;
            attackRequested = false;
            return attackToken;
        }

        public void OnActionWindowOpened(ActionWindowKind kind, int actionToken)
        {
            if (actionToken != attackToken || kind != ActionWindowKind.Damage || IsDead)
            {
                return;
            }

            damageWindowOpen = true;
            weaponHitbox.BeginSwing(definition.attacks[0].damage, gameObject);
        }

        public void OnActionWindowClosed(ActionWindowKind kind, int actionToken)
        {
            if (actionToken != attackToken || kind != ActionWindowKind.Damage)
            {
                return;
            }

            damageWindowOpen = false;
            weaponHitbox.EndSwing();
        }

        public void OnActionStateExited(int actionToken)
        {
            if (actionToken != attackToken)
            {
                return;
            }

            inAttackState = false;
            damageWindowOpen = false;
        }
    }
}
