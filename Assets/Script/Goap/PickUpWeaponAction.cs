using UnityEngine;
using UnityEngine.AI;

public class PickUpWeaponAction : GoapActionBase
{
    NavMeshAgent agent;

    private void Awake()
    {
        preMask = GoapBits.Mask(GoapFact.AtWeapon);
        addMask = GoapBits.Mask(GoapFact.HasWeapon);
        delMask = GoapBits.Mask(GoapFact.AtWeapon);
    }

    public override GoapStatus Tick(GoapContext ctx)
    {
        ctx.Weapon.gameObject.SetActive(false);
        return GoapStatus.Success;
    }
}
