using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinerBB : Blackboard
{


    public Homebase HQ;
    public Asteroid MiningTarget;
    public Vehicle FleeTarget;

    public Vector3 MoveToLocation;
    public float FleeProximity;
    public float Health;



    public bool depositing;
    public bool fleeing;
}
