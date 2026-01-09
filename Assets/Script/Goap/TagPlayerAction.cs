using UnityEngine;
using UnityEngine.AI;

public class TagPlayerAction : GoapActionBase
{
    NavMeshAgent agent;

    private void Awake()
    {
        preMask = GoapBits.Mask(GoapFact.HasWeapon, GoapFact.AtPlayer);
        addMask = GoapBits.Mask(GoapFact.PlayerTagged);
        delMask = 0;
    }

    public override GoapStatus Tick(GoapContext ctx)
    {
        Debug.Log("Tagged the Player");
        return GoapStatus.Success;
    }
}
