using System.Collections.Generic;
using UnityEngine;

namespace HollowSpirit.Gameplay.Player
{
    /// <summary>Picks which lock-on candidate to lock onto and when to let go of it.</summary>
    public static class LockOnSelector
    {
        private const float AngleTieDegrees = 0.01f;

        /// <summary>Returns the index of the best candidate, or -1 when none is in range and in view.</summary>
        public static int Select(Vector3 playerPosition, Vector3 cameraPosition, Vector3 cameraForward,
                                 IReadOnlyList<Vector3> candidates, float maxDistance, float maxViewAngleDegrees)
        {
            int best = -1;
            float bestAngle = 0f;
            float bestSqrDistance = 0f;
            float maxSqrDistance = maxDistance * maxDistance;

            for (int i = 0; i < candidates.Count; i++)
            {
                Vector3 point = candidates[i];
                float sqrDistance = (point - playerPosition).sqrMagnitude;
                if (sqrDistance > maxSqrDistance)
                {
                    continue;
                }

                float angle = Vector3.Angle(cameraForward, point - cameraPosition);
                if (angle > maxViewAngleDegrees)
                {
                    continue;
                }

                bool better = best < 0
                    || angle < bestAngle - AngleTieDegrees
                    || (angle <= bestAngle + AngleTieDegrees && sqrDistance < bestSqrDistance);
                if (better)
                {
                    best = i;
                    bestAngle = angle;
                    bestSqrDistance = sqrDistance;
                }
            }

            return best;
        }

        /// <summary>True when the locked target is farther than the break distance.</summary>
        public static bool ShouldRelease(Vector3 playerPosition, Vector3 targetPoint, float breakDistance)
        {
            return (targetPoint - playerPosition).sqrMagnitude > breakDistance * breakDistance;
        }

        /// <summary>Unit direction from one point to another with the height removed; zero when they line up vertically.</summary>
        public static Vector3 FlatDirection(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.000001f)
            {
                return Vector3.zero;
            }

            return direction.normalized;
        }
    }
}
