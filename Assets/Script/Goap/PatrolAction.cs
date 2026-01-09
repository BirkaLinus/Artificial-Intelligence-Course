using UnityEngine;
using UnityEngine.AI;

public class PatrolAction : GoapActionBase
{
    private NavMeshAgent agent;

    private void Awake()
    {
        preMask = 0;
        addMask = GoapBits.Mask(GoapFact.PatrolStepDone);
        delMask = 0;
    }

    public override void OnEnter(GoapContext ctx)
    {
        agent = ctx.Agent;

        int i = ctx.PatrolIndex % ctx.PatrolWaypoints.Length;
        agent.SetDestination(ctx.PatrolWaypoints[i].position);
    }

    public override GoapStatus Tick(GoapContext ctx)
    {
        if (!agent.pathPending && agent.remainingDistance < 0.02f)
        {
            ctx.PatrolIndex++;
            return GoapStatus.Success;
        }

        return GoapStatus.Running;
    }

}
