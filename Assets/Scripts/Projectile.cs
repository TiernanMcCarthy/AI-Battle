using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteeringBehaviours))]
public class Projectile : MonoBehaviour
{
    //Damage inflicted to target upon impact
    public float Damage;

    //Projectiles will stop existing at the end of their lifespan
    public float LifeSpan;

    //Steering behaviours drive the projectile's movement, notably avoiding objects in the way of its target
    public SteeringBehaviours SB;


    private bool Attacking = false;

    private float StartTime;

    
    public Vehicle VehicleTarget;


    void Start()
    {
        SB = GetComponentInParent<SteeringBehaviours>();
        SB.IsMissile = true;
        SB.ObstacleAvoidanceOn();
    }
    public void Target(Vehicle Attack, bool MyTeam)
    {
        Start(); //These projecitles are instantiated, Start is not called on runtime
        VehicleTarget = Attack;
        SB.SeekOn(Attack.transform.position, 0);
        Attacking = true;
        StartTime = Time.time;
        SB.SetTeam(MyTeam); //Prevent the projectile from attacking team mates
    }

    void Update()
    {
        if (Attacking == true)
        {
            if (VehicleTarget != null)
            {
                SB.SeekOn(VehicleTarget.transform.position);


                if (Time.time - StartTime >= LifeSpan)
                {
                    VehicleTarget.GetComponentInParent<SteeringBehaviours>().ResetBehaviours(); //If the object was avoiding this projectile, it's time to stop running from this projectile
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject); //Clear if designated target is dead
            }
        }
    }


}
