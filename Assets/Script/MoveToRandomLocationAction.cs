using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToRandomLocation", story: "Agent moves to a random [targetLocation]", category: "Action", id: "4502e71eadb9c89d15bc046a4cab611b")]
public partial class MoveToRandomLocationAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;

    protected override Status OnStart()
    {

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

