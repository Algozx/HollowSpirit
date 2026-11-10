using HollowSpirit.Gameplay.Combat;
using Unity.Cinemachine;
using UnityEngine;

namespace HollowSpirit.Gameplay.Player
{
    /// <summary>Switches between the free and lock-on cameras when the player locks or releases a target.</summary>
    [DefaultExecutionOrder(-50)]
    public sealed class LockOnCameraRig : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private CinemachineCamera lockOnCamera;
        [Tooltip("Follow target of the lock-on camera: sits on the player and faces the locked target, so rolls and turns of the character do not swing the camera.")]
        [SerializeField] private Transform followAnchor;
        [Tooltip("Group the lock-on camera aims at: the player plus one slot filled with the target.")]
        [SerializeField] private CinemachineTargetGroup targetGroup;
        [Tooltip("Index of the target group member that holds the locked target.")]
        [SerializeField] private int targetSlot = 1;
        [Tooltip("Mouse and right stick orbit; switched off while locked.")]
        [SerializeField] private CinemachineInputAxisController freeCameraInput;
        [Tooltip("Priority of the lock-on camera while idle and while locked.")]
        [SerializeField] private int idlePriority;
        [SerializeField] private int lockedPriority = 20;

        private Transform slotFallback;
        private bool clearSlotWhenBlendEnds;

        private void Awake()
        {
            if (player == null || lockOnCamera == null || followAnchor == null || targetGroup == null || targetGroup.Targets.Count <= targetSlot)
            {
                Debug.LogError($"{nameof(LockOnCameraRig)}: player, lock-on camera, follow anchor or target group slot are missing on {name}.", this);
                enabled = false;
                return;
            }

            slotFallback = targetGroup.Targets[targetSlot].Object;
        }

        private void OnEnable()
        {
            player.LockedTargetChanged += Apply;
            Apply(player.LockedTarget);
        }

        private void OnDisable()
        {
            player.LockedTargetChanged -= Apply;
        }

        private void Update()
        {
            if (clearSlotWhenBlendEnds && !lockOnCamera.IsLive)
            {
                SetSlot(slotFallback, 0f);
                clearSlotWhenBlendEnds = false;
            }
        }

        private void LateUpdate()
        {
            Transform playerTransform = player.transform;
            Vector3 position = playerTransform.position;
            followAnchor.position = position;

            LockOnTarget target = player.LockedTarget;
            Vector3 facing = target != null
                ? LockOnSelector.FlatDirection(position, target.Point.position)
                : LockOnSelector.FlatDirection(position, position + playerTransform.forward);
            if (facing.sqrMagnitude > 0.0001f)
            {
                followAnchor.rotation = Quaternion.LookRotation(facing, Vector3.up);
            }
        }

        private void Apply(LockOnTarget target)
        {
            bool locked = target != null;
            if (locked)
            {
                SetSlot(target.Point, 1f);
            }

            // Keep aiming at the target until the camera has blended out, so the release does not jump.
            clearSlotWhenBlendEnds = !locked;

            lockOnCamera.Priority.Value = locked ? lockedPriority : idlePriority;
            if (freeCameraInput != null)
            {
                freeCameraInput.enabled = !locked;
            }
        }

        private void SetSlot(Transform member, float weight)
        {
            CinemachineTargetGroup.Target slot = targetGroup.Targets[targetSlot];
            slot.Object = member;
            slot.Weight = weight;
            targetGroup.Targets[targetSlot] = slot;
        }
    }
}
