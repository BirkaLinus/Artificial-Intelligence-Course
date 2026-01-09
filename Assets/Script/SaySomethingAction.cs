using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Say Something", story: "Agent say [LineToSay]", category: "Action", id: "da0b891ca811e45459335d3c3e243a66")]
public partial class SaySomethingAction : Action
{
    [SerializeReference] public BlackboardVariable<string> LineToSay;


    protected override Status OnStart()
    {
        Debug.Log($"LineToSay");

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

