using HollowSpirit.Core.Input;
using HollowSpirit.Gameplay.Animation;
using HollowSpirit.Gameplay.Combat;
using HollowSpirit.Gameplay.Data;
using UnityEngine;

namespace HollowSpirit.Gameplay.Player
{
    /// <summary>Reads input, drives the action state machine and Animator, and moves the character with root motion.</summary>
    [RequireComponent(typeof(CharacterController), typeof(Animator))]
    public sealed class PlayerController : MonoBehaviour, IActionWindowReceiver, IDamageable
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int RollHash = Animator.StringToHash("Roll");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");

        [SerializeField] private CharacterStats stats;
        [SerializeField] private RollData rollData;
        [Tooltip("Light attack chain in order; after the last one the chain starts over.")]
        [SerializeField] private AttackData[] lightAttacks;
        [SerializeField] private WeaponHitbox weaponHitbox;
        [SerializeField] private InputTuning inputTuning;
        [Tooltip("Camera used for camera-relative movement. Falls back to the main camera.")]
        [SerializeField] private Transform cameraTransform;

        private HollowSpiritInput input;
        private InputBuffer buffer;
        private Animator animator;
        private CharacterController characterController;
        private float verticalVelocity;
        private Vector3 moveDirection;

        public Stamina Stamina { get; private set; }
        public Health Health { get; private set; }
        public PlayerActionStateMachine Actions { get; private set; }
        public bool IsInvulnerable => Actions.IsInvulnerable;

        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                Debug.LogError($"{nameof(PlayerController)}: stats, roll, input tuning, weapon hitbox or light attacks are missing on {name}.", this);
                enabled = false;
                return;
            }

            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            input = new HollowSpiritInput();
            buffer = new InputBuffer(inputTuning.bufferWindowSeconds);
            Stamina = new Stamina(stats.maxStamina, stats.staminaRegenPerSecond, stats.staminaRegenDelaySeconds);
            Health = new Health(stats.maxHealth);
            Actions = new PlayerActionStateMachine(buffer, Stamina, rollData, lightAttacks, Health);
            Actions.RollStarted += HandleRollStarted;
            Actions.AttackStarted += HandleAttackStarted;

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private bool HasRequiredReferences()
        {
            if (stats == null || rollData == null || inputTuning == null || weaponHitbox == null || lightAttacks == null || lightAttacks.Length == 0)
            {
                return false;
            }

            foreach (AttackData attack in lightAttacks)
            {
                if (attack == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnEnable()
        {
            input?.Gameplay.Enable();
        }

        private void OnDisable()
        {
            input?.Gameplay.Disable();
        }

        private void OnDestroy()
        {
            if (Actions != null)
            {
                Actions.RollStarted -= HandleRollStarted;
                Actions.AttackStarted -= HandleAttackStarted;
            }

            input?.Dispose();
        }

        private void Update()
        {
            float time = Time.time;
            float deltaTime = Time.deltaTime;

            if (input.Gameplay.Roll.WasPressedThisFrame())
            {
                buffer.Press(BufferedAction.Roll, time);
            }

            if (input.Gameplay.Attack.WasPressedThisFrame())
            {
                buffer.Press(BufferedAction.Attack, time);
            }

            Stamina.Tick(deltaTime, time);

            Vector2 stick = Vector2.ClampMagnitude(input.Gameplay.Move.ReadValue<Vector2>(), 1f);
            moveDirection = ToCameraRelative(stick);

            Actions.Update(time);

            bool moving = Actions.State == PlayerActionState.Move;
            animator.SetFloat(SpeedHash, moving ? stick.magnitude : 0f, stats.speedDampSeconds, deltaTime);

            if (moving && moveDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, stats.turnSpeedDegrees * deltaTime);
            }
        }

        private void OnAnimatorMove()
        {
            Vector3 delta = animator.deltaPosition;
            delta.y = 0f;

            if (characterController.isGrounded)
            {
                verticalVelocity = -stats.groundStickSpeed;
            }
            else
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            delta.y = verticalVelocity * Time.deltaTime;
            characterController.Move(delta);
        }

        public int OnActionStateEntered()
        {
            return Actions.BeginAction();
        }

        public void OnActionWindowOpened(ActionWindowKind kind, int actionToken)
        {
            bool accepted = Actions.OpenWindow(kind, actionToken);
            if (accepted && kind == ActionWindowKind.Damage && Actions.CurrentAttack != null)
            {
                weaponHitbox.BeginSwing(Actions.CurrentAttack.damage, gameObject);
            }
        }

        public void OnActionWindowClosed(ActionWindowKind kind, int actionToken)
        {
            bool accepted = Actions.CloseWindow(kind, actionToken);
            if (accepted && kind == ActionWindowKind.Damage)
            {
                weaponHitbox.EndSwing();
            }
        }

        public void OnActionStateExited(int actionToken)
        {
            Actions.NotifyActionFinished(actionToken);
            if (Actions.State == PlayerActionState.Move)
            {
                animator.ResetTrigger(AttackHash);
                animator.ResetTrigger(RollHash);
            }
        }

        public bool TryTakeHit(in HitInfo hit)
        {
            return Actions.TryTakeHit(hit);
        }

        /// <summary>Hit animation event shipped inside the attack clips; damage follows the damage window, not this event.</summary>
        public void Hit()
        {
        }

        /// <summary>Footstep animation event shipped inside the locomotion clips; audio hooks in later.</summary>
        public void FootL()
        {
        }

        /// <summary>Footstep animation event shipped inside the locomotion clips; audio hooks in later.</summary>
        public void FootR()
        {
        }

        private void HandleRollStarted()
        {
            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            }

            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(RollHash);
        }

        private void HandleAttackStarted(AttackData attack, int chainIndex)
        {
            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            }

            animator.SetInteger(AttackIndexHash, chainIndex);
            animator.SetTrigger(AttackHash);
        }

        private Vector3 ToCameraRelative(Vector2 stick)
        {
            if (cameraTransform == null || stick.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();
            return forward * stick.y + right * stick.x;
        }
    }
}
