using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnHome :State
{


    public override void Execute(StateObject s)
    {
        Fighter F = (Fighter)s;
        if (F.FriendlyBase != null)
        {
            F.SB.ArriveOn(F.FriendlyBase.transform.position, 4.0f, 0);

            if (Vector3.Distance(F.FriendlyBase.transform.position, F.transform.position) < 5.0f && F.CanIHeal() == true)
            {
                F.SB.ResetBehaviours();
                F.AttachedVehicle.Health = F.AttachedVehicle.MaximumHealth;
                F.AttachedVehicle.Ammo = F.AttachedVehicle.MaximumAmmo;
                F.LocalHeal = F.HealTime; //The ship cannot heal if it is damaged too much before the 30 second period
                F.LastHeal = Time.time;
            }
            else if (Vector3.Distance(F.FriendlyBase.transform.position, F.transform.position) < 5.0f && F.CanIHeal() == false)
            {
                F.AttachedVehicle.Scan();
                F.CurrentState = new Attack();
            }
            else
            {
                F.CurrentState = new FighterEvaluate();
            }
        }
    }
}
