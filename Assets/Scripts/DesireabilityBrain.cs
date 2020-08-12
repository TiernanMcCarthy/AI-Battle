using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesireabilityBrain 
{
    public float WorthWhileDistance = 30.0f; //Distance at which combat is worthwhile in taking place
    
    private Vehicle MainActor; //The agent that is being processed in this iteration

    private Vehicle SecondaryActor; //Potential Enemy target that is being processed

    private Vehicle Homebase; //Mother base that is to be defended by this agent

    //Fighter Specific Desires
    public float GetHealthDesire; //Desireability values that fuel agent decisions based on environmental stimulus and current realities
    public float GetAmmoDesire;
    public float GetPowerfulness;

    //Homebase Resource Desires
    public float MinerDesire;
    public float FighterDesire;


    public const float k = 0.5f; //Limiter on these desireability values in an attempt to keep them within meaningful range
    // Start is called before the first frame update

    public DesireabilityBrain(Vehicle Actor,Vehicle SecondActor,Vehicle h)
    {
        MainActor = Actor;
        SecondaryActor = SecondActor;
        Homebase = h;
    }
    public DesireabilityBrain()
    {

    }

    public void Set(Vehicle Actor, Vehicle SecondActor, Vehicle h)
    {
        MainActor = Actor;
        SecondaryActor = SecondActor;
        Homebase = h;
    }

    private float HealthStatus(Vehicle Test)
    {
        if(Test.Health/Test.MaximumHealth>0.75f) //More than 75% of health is not worth factoring
        {
            return 1; //Healthy
        }

        return Test.Health/ Test.MaximumHealth ; //Get Agent Health ratio
    }

    //Ships get ammo and health from the motherbase which is an "agent". This works for both sides
    private float DistanceToAgent(Vehicle PlayerAgent, Vehicle DesiredAgent)
    {
        if (PlayerAgent != null && DesiredAgent != null) //If the target is within a range the ship should return a ratio or that the event is not worthwhile
        {
            if (Vector3.Distance(PlayerAgent.transform.position, DesiredAgent.transform.position) > WorthWhileDistance)
            {
                return 1;
            }
            else
            {
                return Vector3.Distance(PlayerAgent.transform.position, DesiredAgent.transform.position) / WorthWhileDistance;
            }
        }
        return 0; //No target, do not proceed
    }

    private float WeaponAmmo(Vehicle Test) 
    {
        if (Test.Ammo == 0 && Test.MaximumAmmo != 0) //IF this vehicle is out of ammo and is actually an ammo carrying type
        {
            return 1; //A weaponless combat vehicle is useless, rearm now
        }
        else if (Test.Ammo == 0 && Test.MaximumAmmo == 0) //This vehicle doesn't carry ammo, it shouldn't return to rearm
        {
            return 0;
        }
        return Test.Ammo / Test.MaximumAmmo; //Get a desireability ratio based on how much ammo is remaining. The desire to shoot a nearby target should be higher than this
    }

    private bool  VisbilityOnTarget(Vehicle Test)
    {
        if(Test.TargetVehicle!=null) //If the target has been picked up by sensors or is previously known, return 1
        {
            return true;
        }
        return false;

    }




    public void Check()
    {
        if (this != null)
        {


            GetHealthDesire = k * ((1 - HealthStatus(MainActor)) / Mathf.Pow(DistanceToAgent(MainActor, Homebase), 2));
            GetAmmoDesire = k * ((HealthStatus(MainActor) * (1 - WeaponAmmo(MainActor))) / Mathf.Pow(DistanceToAgent(MainActor, Homebase), 2));
           
            if (VisbilityOnTarget(MainActor)) //If the main target is seen, work out how this agent would fair from its condition
            {
                GetPowerfulness = WeaponAmmo(MainActor) * HealthStatus(MainActor); //Addition prevents the ship from jumping into a heal state it cannot do
            } 
        }
    }

    private float GetMinerDesireability(Homebase h) //Determine if a new miner needs to be built or not
    {
        float CurrentRatio = h.Miners.Count /h.MaxMiners; //Maintain a suitable ratio of miners to a max and wether fighters should be built

        if(CurrentRatio>0.9)
        {
            return 0;
        }
        else if(CurrentRatio<=0.7f)
        {
            return 1;
        }

        return 1 - CurrentRatio; //Get a desire based upon the remaining amount required;
    }

    private float GetFighterDesireability(Homebase h) //Decide if a fighter should be built currently
    {
        float CurrentRatio = h.Fighters.Count / h.MaxFighters;

        return 1 - CurrentRatio; //Get a desire based upon the remaining amount
    }

    private float MoneyToSpend(Homebase h, char Type) //M is Miner, F is fighter
    {
        switch (Type) //Check which type needs to be built and determine if the required resources are available
        {
            case 'M':
                if (h.StoredResources >= h.MinerCost)
                {
                    return 1;
                }
                break;
            case 'F':
                if (h.StoredResources >= h.FighterCost)
                {
                    return 1;
                }
                break;
        }
        return 0;
     
    }

    public void Check(Homebase h) //Homebase check for what vehicle needs to be built
    {
        if (h.BuildMiners == true && h.MiningTarget.GoldCount!=0)
        {
            MinerDesire = GetMinerDesireability(h) * MoneyToSpend(h, 'M');
        }
        FighterDesire = GetFighterDesireability(h) * MoneyToSpend(h, 'F');



    }

}
