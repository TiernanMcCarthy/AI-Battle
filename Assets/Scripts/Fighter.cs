    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteeringBehaviours))]
public class Fighter : StateObject
{


    public Vehicle AttachedVehicle; //The vehicle is the Unity GameObject that this script drives

    public SteeringBehaviours SB; //Steering behaviours dictate how this vehicle moves

    //Weapon Attributes
    public float FireRate;

    public float FireRange;

    public float FireDamage;

    //Enemy Target Vehicle
    public Vehicle Enemy;

    //This object funtions through a finite state machine approach, the current state is executed once each frame
    public State CurrentState;

    public float LastFireTime;

    //Homebase targets where the unit can attack and rearm
    public Homebase EnemyBase;

    public Homebase FriendlyBase;

    //Missile GameObject Generated on firing
    public Projectile MissilePrefab;

    public float HealTime = 30;

    public  float LocalHeal = 0;

    public float LastHeal = 0;

    public bool CanIHeal()
    {

        if(Time.time - LastHeal>= LocalHeal)
        {
            return true;
        }
        //LastHeal = Time.time;
        return false;

    }
    // Start is called before the first frame update
    void Initialise()
    {
        //Get references to components to speed up access
        AttachedVehicle = GetComponentInParent<Vehicle>();
        SB = GetComponentInParent<SteeringBehaviours>();
        AttachedVehicle.Parent = this;

        Homebase[] HBLIST = FindObjectsOfType<Homebase>();

        if(HBLIST[0].Team!=Team)
        {
            EnemyBase = HBLIST[0];
        }

        CurrentState = new ScanState();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentState != null)
            CurrentState.Execute(this);
        Enemy = AttachedVehicle.TargetVehicle;
    }

    public override Vehicle Target(List<Vehicle> t)
    {
        throw new System.NotImplementedException();
    }

    public override void SetParentTarget(Vehicle t)
    {
        Enemy = t;
    }
    public void InstansiateStart()
    {
        Initialise();
    }

    public void Start()
    {
        Initialise();
    }
    public void Fire() //Spawn Bullet Prefab
    {

        Projectile Temp = Instantiate(MissilePrefab,transform.position +transform.forward*2,Quaternion.identity);
        AttachedVehicle.Ammo--;
        Temp.Target(Enemy, Team);
        Temp.Damage = FireDamage;
    }


}
