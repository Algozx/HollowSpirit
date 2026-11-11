using HollowSpirit.Gameplay.Data;
using Unity.Cinemachine;
using UnityEngine;

namespace HollowSpirit.Gameplay.Player
{
    /// <summary>Waits after the player dies, then moves them to the respawn point and stands them back up.</summary>
    public sealed class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private DeathFlowTuning tuning;
        [SerializeField] private Transform respawnPoint;

        private CharacterController playerController;
        private float respawnAtTime;
        private bool waiting;

        private void Awake()
        {
            if (player == null || tuning == null || respawnPoint == null)
            {
                Debug.LogError($"{nameof(PlayerRespawn)}: player, tuning or respawn point is missing on {name}.", this);
                enabled = false;
                return;
            }

            playerController = player.GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (player != null)
            {
                player.Died += HandlePlayerDied;

                // Re-enabled while the player is still down: start the wait again instead of leaving them dead.
                if (player.IsDead && !waiting)
                {
                    HandlePlayerDied();
                }
            }
        }

        private void OnDisable()
        {
            if (player != null)
            {
                player.Died -= HandlePlayerDied;
            }

            waiting = false;
        }

        private void Update()
        {
            if (waiting && Time.time >= respawnAtTime)
            {
                waiting = false;
                Respawn();
            }
        }

        private void HandlePlayerDied()
        {
            respawnAtTime = Time.time + tuning.respawnDelaySeconds;
            waiting = true;
        }

        private void Respawn()
        {
            // A CharacterController overrides transform changes unless it is off while teleporting.
            Vector3 from = player.transform.position;
            playerController.enabled = false;
            player.transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
            playerController.enabled = true;

            // Cameras following the player jump with it instead of flying across the level.
            CinemachineCore.OnTargetObjectWarped(player.transform, respawnPoint.position - from);
            player.Respawn();
        }
    }
}
