using HollowSpirit.Core.Input;
using HollowSpirit.Gameplay.Animation;
using HollowSpirit.Gameplay.Combat;
using HollowSpirit.Gameplay.Data;
using UnityEngine;

namespace HollowSpirit.Gameplay.Player
{
    /// <summary>Reads input, drives the action state machine and Animator, and moves the character with root motion.</summary>
    [RequireComponent(typeof(CharacterController), typeof(Animator))]
    public sealed class PlayerController : MonoBehaviour, IActionWindowReceiver
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int RollHash = Animator.StringToHash("Roll");

        [SerializeField] private CharacterStats stats;
        [SerializeField] private RollData rollData;
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
        public PlayerActionStateMachine Actions { get; private set; }
        public bool IsInvulnerable => Actions.IsInvulnerable;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            input = new HollowSpiritInput();
            buffer = new InputBuffer(inputTuning.bufferWindowSeconds);
            Stamina = new Stamina(stats.maxStamina, stats.staminaRegenPerSecond, stats.staminaRegenDelaySeconds);
            Actions = new PlayerActionStateMachine(buffer, Stamina, rollData);
            Actions.RollStarted += HandleRollStarted;

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            input.Gameplay.Enable();
        }

        private void OnDisable()
        {
            input.Gameplay.Disable();
        }

        private void OnDestroy()
        {
            if (Actions != null)
            {
                Actions.RollStarted -= HandleRollStarted;
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

        public void OnActionWindowOpened(ActionWindowKind kind)
        {
            Actions.OpenWindow(kind);
        }

        public void OnActionWindowClosed(ActionWindowKind kind)
        {
            Actions.CloseWindow(kind);
        }

        public void OnActionStateExited()
        {
            Actions.NotifyActionFinished();
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

            animator.SetTrigger(RollHash);
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
