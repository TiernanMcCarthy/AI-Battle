using System.Collections.Generic;
using UnityEngine;
public class ScanState : State
{

    public override void Execute(StateObject s)
    {
        Vehicle v = s.GetComponentInParent<Vehicle>();
        Collider[] hitList=  Physics.OverlapSphere(v.transform.position, v.ScanRange);
        List<SteeringBehaviours.ID> ClosestPositions = new List<SteeringBehaviours.ID>();
        if (hitList.Length > 1)
        {
            for (int i = 0; i < hitList.Length; i++)
            {
                if (hitList[i].gameObject != s.gameObject && hitList[i].gameObject.tag == "Vehicle")
                {

                    if (hitList[i].gameObject.GetComponentInParent<Vehicle>().team != v.team)
                    {
                        SteeringBehaviours.ID Close = new SteeringBehaviours.ID(i, v.transform.position);

                        ClosestPositions.Add(Close);
                    }
                }
            }

            if (ClosestPositions.Count != 0)
            {

                if (ClosestPositions.Count > 1)
                {
                    for (int i = 0; i < ClosestPositions.Count - 1; i++) //Iterate through these and find the closest position by sorting
                    {


                        if (Vector3.Distance(ClosestPositions[i].Position, v.transform.localPosition) > Vector3.Distance(ClosestPositions[i + 1].Position, v.transform.localPosition))
                        {
                            SteeringBehaviours.ID temp = ClosestPositions[i];
                            ClosestPositions[i] = ClosestPositions[i + 1]; //Swap these positions
                            ClosestPositions[i + 1] = temp;
                            i = -1; //Reiterate
                        }

                    }
                }
                v.SetVehicle(hitList[ClosestPositions[0].Index].gameObject); //set the target vehicle
            }

        }
        else
        {
            if (s.GetComponent<Fighter>())
            {
                Fighter F = (Fighter)s;
                if (F.EnemyBase == null || F.AttachedVehicle.EnemyBase==null)
                {
                    F.Enemy = F.FriendlyBase.OpposingEnemy.BaseObject;
                    F.AttachedVehicle.EnemyBase = F.FriendlyBase.OpposingEnemy;
                }
            }
        }
        if (s.GetComponent<Fighter>())
        {
            Fighter F = (Fighter)s;
            F.CurrentState = new FighterEvaluate();
        }

    }
}
