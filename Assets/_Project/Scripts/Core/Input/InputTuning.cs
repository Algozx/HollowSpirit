using UnityEngine;

namespace HollowSpirit.Core.Input
{
    /// <summary>Tuning values for player input.</summary>
    [CreateAssetMenu(menuName = "HollowSpirit/Input Tuning")]
    public sealed class InputTuning : ScriptableObject
    {
        [Tooltip("How long a buffered action stays valid, in seconds.")]
        public float bufferWindowSeconds;
    }
}
