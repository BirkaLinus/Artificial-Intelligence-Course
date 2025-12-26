using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
namespace GameAI.Lab4
{
/// <summary>
/// Custom Unity Behavior Action node:
/// - Reads GuardSensors
/// - Writes to blackboard: Target, HasLineOfSight, LastKnownPosition, TimeSinceLastSeen
///
/// NOTE: Best workflow:
/// 1) Create this node via the Behavior Graph editor (Create new -> Action),
/// 2) Then replace the generated script content with this file.
/// </summary>
[Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Update Perception",description: "Updates Target/LOS/LastKnownPosition fromGuardSensors.",story: "Update perception and write to the blackboard.",category: "Action/Sensing",id: "b0c8b9b7-8f64-4c0c-a9a0-0d9e7c2e2fb8")]
    public class UpdatePerception : Unity.Behavior.Action
    {
        [SerializeReference]
        public BlackboardVariable<GameObject>
        Target;
        [SerializeReference]
        public BlackboardVariable<bool>
        HasLineOfSight;
        [SerializeReference]
        public BlackboardVariable<Vector3>
        LastKnownPosition;
        [SerializeReference]
        public BlackboardVariable<float>
        TimeSinceLastSeen;
        protected override Status OnStart()
        {
            // Ensure we have sane defaults.
            if (TimeSinceLastSeen != null && TimeSinceLastSeen.Value
            < 0f)
                TimeSinceLastSeen.Value = 9999f;
            return Status.Success;
        }
        protected override Status OnUpdate()
        {
            var sensors = GameObject != null ? GameObject.GetComponent<GuardSensors>() : null;
            if (sensors == null)
            {
                // No sensors attached -> treat as "can't see anything"
            if (HasLineOfSight != null) HasLineOfSight.Value =
            false;
                if (TimeSinceLastSeen != null)
                    TimeSinceLastSeen.Value += Time.deltaTime;
                return Status.Success;
            }
            bool sensed = sensors.TrySenseTarget(
            out GameObject sensedTarget,
            out Vector3 sensedPos,
            out bool hasLOS
            );
            if (sensed && hasLOS)
            {
                if (Target != null) Target.Value = sensedTarget;
                if (HasLineOfSight != null) HasLineOfSight.Value =
                true;
                if (LastKnownPosition != null)
                    LastKnownPosition.Value = sensedPos;
                if (TimeSinceLastSeen != null)
                    TimeSinceLastSeen.Value = 0f;
            }
            else
            {
                // Keep Target as-is (we "remember" who we were chasing),
                // but mark that we don't currently have LOS.
                if (HasLineOfSight != null) HasLineOfSight.Value = false;
                if (TimeSinceLastSeen != null)
                    TimeSinceLastSeen.Value += Time.deltaTime;
            }
            // This node is a fast "service-like" update; it finishes immediately each tick.
            return Status.Success;
        }
    }
}
