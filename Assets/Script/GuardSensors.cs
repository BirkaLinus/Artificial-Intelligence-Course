using UnityEngine;
namespace GameAI.Lab4
{
    /// <summary>
    /// Perception helper: detects a Player target in range/FOV and checks line-of-sight via raycast.
/// The BT uses this through a custom action node.
/// </summary>
public class GuardSensors : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private string targetTag = "Player";
        [Header("View")]
        [SerializeField] private float viewDistance = 10f;
        [Range(1f, 180f)]
        [SerializeField] private float viewAngleDegrees = 90f;
        [Header("Line of Sight")]
        [SerializeField] private Transform eyes;
        [SerializeField] private LayerMask occlusionMask = ~0; //everything by default
private Transform cachedTarget;
        public float ViewDistance => viewDistance;
        public float ViewAngleDegrees => viewAngleDegrees;
        private Transform EyesTransform => eyes != null ? eyes :
        transform;
        private void Awake()
        {
            // Cache once; good enough for a lab. (You can improve later.)
GameObject go =
GameObject.FindGameObjectWithTag(targetTag);
            cachedTarget = go != null ? go.transform : null;
        }
        public bool TrySenseTarget(out GameObject target, out
        Vector3 lastKnownPosition, out bool hasLineOfSight)
        {
            target = null;
            lastKnownPosition = default;
            hasLineOfSight = false;
            if (cachedTarget == null) return false;
            Vector3 eyePos = EyesTransform.position;
            Vector3 toTarget = cachedTarget.position - eyePos;
            float dist = toTarget.magnitude;
            if (dist > viewDistance) return false;
            Vector3 toTargetDir = toTarget / Mathf.Max(dist,
            0.0001f);
            float halfAngle = viewAngleDegrees * 0.5f;
            float angle = Vector3.Angle(EyesTransform.forward,
            toTargetDir);
            if (angle > halfAngle) return false;
            // Raycast to check occlusion
            if (Physics.Raycast(eyePos, toTargetDir, out RaycastHit
            hit, dist, occlusionMask))
            {
                // If we hit something BEFORE reaching the target, it's blocked.
            if (hit.transform != cachedTarget)
                {
                    return false;
                }
            }
            target = cachedTarget.gameObject;
            lastKnownPosition = cachedTarget.position;
            hasLineOfSight = true;
            return true;
        }
    }
}