using UnityEngine;

namespace HollowSpirit.Gameplay.Combat
{
    /// <summary>Sweeps a capsule along the blade while a damage window is open and hits each target once per swing.</summary>
    public sealed class WeaponHitbox : MonoBehaviour
    {
        private const int BufferSize = 16;
        private const int MaxSubsteps = 12;
        private const int RegistryCapacity = 16;

        [SerializeField] private Transform bladeBase;
        [SerializeField] private Transform bladeTip;
        [SerializeField] private float radius;
        [SerializeField] private LayerMask hurtboxMask;
        [Tooltip("Draws the capsule in the Scene view and as debug lines while a swing is active.")]
        [SerializeField] private bool drawDebug;

        private readonly Collider[] overlaps = new Collider[BufferSize];
        private readonly HitRegistry registry = new HitRegistry(RegistryCapacity);
        private GameObject attacker;
        private float damage;
        private bool swingActive;
        private bool checkPending;
        private Vector3 previousBase;
        private Vector3 previousTip;

        public bool SwingActive => swingActive;

        /// <summary>The damage window opened; the next late update checks the blade at least once.</summary>
        public void BeginSwing(float swingDamage, GameObject swingAttacker)
        {
            damage = swingDamage;
            attacker = swingAttacker;
            registry.Reset();
            swingActive = true;
            checkPending = true;
        }

        /// <summary>The damage window closed; a pending check from the same frame still runs once.</summary>
        public void EndSwing()
        {
            swingActive = false;
        }

        /// <summary>Capsule checks needed so consecutive blade poses are at most one radius apart.</summary>
        public static int SubstepCount(float maxDisplacement, float radius, int maxSubsteps)
        {
            if (radius <= 0f || maxDisplacement <= radius)
            {
                return 1;
            }

            return Mathf.Clamp(Mathf.CeilToInt(maxDisplacement / radius), 1, maxSubsteps);
        }

        private void Awake()
        {
            if (bladeBase == null || bladeTip == null)
            {
                Debug.LogError($"{nameof(WeaponHitbox)}: blade base or tip is not assigned on {name}.", this);
                enabled = false;
                return;
            }

            previousBase = bladeBase.position;
            previousTip = bladeTip.position;
        }

        private void LateUpdate()
        {
            Vector3 currentBase = bladeBase.position;
            Vector3 currentTip = bladeTip.position;

            if (swingActive || checkPending)
            {
                checkPending = false;
                SweepHits(currentBase, currentTip);
            }

            previousBase = currentBase;
            previousTip = currentTip;
        }

        // The blade tip moves up to about 2 m per frame at 60 fps, so the capsule is also checked at
        // poses interpolated between last frame and this one; otherwise it can pass through a target.
        private void SweepHits(Vector3 currentBase, Vector3 currentTip)
        {
            float displacement = Mathf.Max(Vector3.Distance(previousBase, currentBase), Vector3.Distance(previousTip, currentTip));
            int steps = SubstepCount(displacement, radius, MaxSubsteps);
            for (int step = 1; step <= steps; step++)
            {
                float t = (float)step / steps;
                CheckHits(Vector3.Lerp(previousBase, currentBase, t), Vector3.Lerp(previousTip, currentTip, t));
            }
        }

        private void CheckHits(Vector3 start, Vector3 end)
        {
#if UNITY_EDITOR
            if (drawDebug)
            {
                DrawCapsule(start, end);
            }
#endif

            int count = Physics.OverlapCapsuleNonAlloc(start, end, radius, overlaps, hurtboxMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                Collider other = overlaps[i];
                if (attacker != null && other.transform.IsChildOf(attacker.transform))
                {
                    continue;
                }

                IDamageable target = other.GetComponentInParent<IDamageable>();
                if (target == null || !registry.TryRegister(target))
                {
                    continue;
                }

                target.TryTakeHit(new HitInfo(damage, attacker, other.ClosestPoint(end)));
            }
        }

#if UNITY_EDITOR
        private void DrawCapsule(Vector3 start, Vector3 end)
        {
            Debug.DrawLine(start, end, Color.red, 2f);
            Vector3 axis = (end - start).normalized;
            Vector3 side = Vector3.Cross(axis, Vector3.up);
            if (side.sqrMagnitude < 0.0001f)
            {
                side = Vector3.right;
            }

            side = side.normalized * radius;
            Vector3 up = Vector3.Cross(axis, side).normalized * radius;
            Debug.DrawLine(start + side, end + side, Color.red, 2f);
            Debug.DrawLine(start - side, end - side, Color.red, 2f);
            Debug.DrawLine(start + up, end + up, Color.red, 2f);
            Debug.DrawLine(start - up, end - up, Color.red, 2f);
        }
#endif

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawDebug || bladeBase == null || bladeTip == null)
            {
                return;
            }

            Gizmos.color = swingActive ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(bladeBase.position, radius);
            Gizmos.DrawWireSphere(bladeTip.position, radius);
            Gizmos.DrawLine(bladeBase.position, bladeTip.position);
        }
#endif
    }
}
