using UnityEngine;
using UnityEngine.AI;

public class MoveToWeaponAction : GoapActionBase
{
    private NavMeshAgent agent;

    private void Awake()
    {
        preMask = GoapBits.Mask(GoapFact.WeaponExists);
        addMask = GoapBits.Mask(GoapFact.AtWeapon);
        delMask = 0;
    }

    public override void OnEnter(GoapContext ctx)
    {
        agent = ctx.Agent;
        agent.SetDestination(ctx.Weapon.position);
    }

    public override GoapStatus Tick(GoapContext ctx)
    {
        if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            return GoapStatus.Success;
        }

        return GoapStatus.Running;
    }
}
