using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

public class MoveToPlayerAction : GoapActionBase
{
    NavMeshAgent agent;

    private void Awake()
    {
        preMask = GoapBits.Mask(GoapFact.SeesPlayer);
        addMask = GoapBits.Mask(GoapFact.AtPlayer);
        delMask = 0;
    }

    public override void OnEnter(GoapContext ctx)
    {
        agent = ctx.Agent;
    }

    public override GoapStatus Tick(GoapContext ctx)
    {
        if (!ctx.Sensors.SeesPlayer) return GoapStatus.Failure;

        agent.SetDestination(ctx.Player.position);

        if (!agent.pathPending && agent.remainingDistance < 1.2f) return GoapStatus.Success;

        return GoapStatus.Running;
    }

}
