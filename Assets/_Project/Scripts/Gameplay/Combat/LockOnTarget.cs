using System.Collections.Generic;
using UnityEngine;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Marks an object the player can lock onto and keeps a registry of the enabled ones.</summary>
    public sealed class LockOnTarget : MonoBehaviour
    {
        private static readonly List<LockOnTarget> Registry = new List<LockOnTarget>();

        [Tooltip("Where the camera and the player aim, at chest height.")]
        [SerializeField] private Transform point;

        /// <summary>Every enabled lock-on target; do not modify.</summary>
        public static IReadOnlyList<LockOnTarget> Active => Registry;

        public Transform Point => point != null ? point : transform;

        private void OnEnable()
        {
            Registry.Add(this);
        }

        private void OnDisable()
        {
            Registry.Remove(this);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            Registry.Clear();
        }
    }
}
