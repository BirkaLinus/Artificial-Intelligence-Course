using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace GameAI.Lab4
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Clear Target",
        description: "Clears Target and resets LOS memory.",
        story: "Forget the target and reset perception flags.",
        category: "Action/Sensing",
        id: "d1b6d9f2-7c9b-4e7b-a8e5-4c4ff1c2a4dd"
    )]
    public class ClearTarget : Unity.Behavior.Action
    {
        [SerializeReference]
        public BlackboardVariable<GameObject>Target;
        [SerializeReference]
        public BlackboardVariable<bool>HasLineOfSight;
        [SerializeReference]
        public BlackboardVariable<float>TimeSinceLastSeen;

        protected override Node.Status OnUpdate()
        {
            if (Target != null) Target.Value = null;
            if (HasLineOfSight != null) HasLineOfSight.Value =false;
            if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value =9999f;

            return Node.Status.Success;
        }
    }
}