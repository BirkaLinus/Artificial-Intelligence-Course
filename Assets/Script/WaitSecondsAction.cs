using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitSeconds", story: "Wait for [WaitTime] seconds.", category: "Action", id: "8d769905ed2c2221e69eeac3d97d7b18")]
public partial class WaitSecondsAction : Action
{
    [SerializeReference] public BlackboardVariable<float> WaitTime;

    private float startTime;

    protected override Status OnStart()
    {
        startTime = Time.time;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Time.time - startTime >= WaitTime)
        {
            return Status.Success;
        }

        return Status.Running;

    }
}

