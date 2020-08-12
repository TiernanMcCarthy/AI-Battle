using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : State
{

    public override void Execute(StateObject s)
    {
        Fighter F = (Fighter)s;

        if (F.Enemy != null && Vector3.Distance(F.Enemy.transform.position, F.transform.position) <= F.FireRange) //If the enemy is within range
        {
            F.SB.ResetBehaviours(); //Halt Steering Behaviours, and use this as a firing position
            if (Time.time - F.LastFireTime >= F.FireRate)
            {
                if (F.AttachedVehicle.Ammo != 0)
                {
                    F.LastFireTime = Time.time;
                    F.Fire();
                    F.CurrentState = new FighterEvaluate(); //See what the fighter should do next

                }
                else
                {
                    //F.CurrentState = new ScanState();
                }


            }
        }
        else if(F.Enemy!=null)
        {
            F.SB.ArriveOn(F.Enemy.transform.position, F.FireRange * 0.9f, 0); //The enemy is out of range, arrive in front of the target to fire
        }

        F.CurrentState = new FighterEvaluate(); //See what the fighter should do next

    }
}
