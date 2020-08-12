using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vehicle))]
public class Homebase : StateObject
{

    //Resources collected by miners
    public int StoredResources = 0;

    
    public int MinerCost;

    public int FighterCost;

    //Prefab objects for spawning in units depending on situation and funds
    public Miner MinerPrefab;

    public Fighter FighterPrefab;

    //All miners and fighters that this base has created
    public List<Miner> Miners;

    public List<Fighter> Fighters;

    //The Unity Gameobject this script is attached to
    public Vehicle BaseObject;

    //This object executes its current state every frame to achieve its goals.
    public State CurrentState;

    //Asteroid target for Miners
    public Asteroid MiningTarget;

    //Limits on unit spawning. These are low for balance and funtionality more than anything else, otherwise it's a bit crazy
    public float MaxMiners = 3;
    public float MaxFighters = 9;

    public Homebase OpposingEnemy; //Target Homebase to attack

    private float PreviousHealth;

    public Material Colour;


    public bool BuildMiners = true; //Set to false once resources are depleted
    // Start is called before the first frame update
    void Start()
    {
        BaseObject = GetComponentInParent<Vehicle>();

        Fighters = new List<Fighter>();
        Miners = new List<Miner>();

        SpawnMiner(); //All bases start off with a miner and a fighter
        SpawnFighter();

        PreviousHealth = BaseObject.Health;
    }


    public float GetPreviousHealth()
    {
        float Previous = PreviousHealth;
        PreviousHealth = BaseObject.Health;
        return Previous;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentState = new BuildState();
      

        CurrentState.Execute(this);
    }

    public void Deposit(int Amount)
    {
        StoredResources += Amount;
    }


    public override void SetParentTarget(Vehicle t)
    {
        throw new System.NotImplementedException();
    }

    public void SpawnMiner() //Spawn and assign all the behaviour a miner needs
    {
        Miner Temp = Instantiate(MinerPrefab, transform.position + transform.forward * 2, Quaternion.identity);
        Temp.Team = Team;
        Temp.InstansiateStart();
        Temp.AttachedVehicle = Temp.GetComponentInParent<Vehicle>();
        Temp.AttachedVehicle.team = Team;
        Temp.HQ = this;
        Temp.AttachedVehicle.MaxSpeed = 5;
        Temp.MiningTarget = MiningTarget;
        Temp.SB.ObstacleAvoidanceOn();
        Temp.GetComponent<Renderer>().material = Colour;
        Miners.Add(Temp);
    }


   

    public void SpawnFighter() //Spawn and assign all the behaviour a fighter needs
    {
        Fighter Temp = Instantiate(FighterPrefab, transform.position + transform.right * 2, Quaternion.identity);
        Temp.Team = Team;
        Temp.InstansiateStart();
        Temp.AttachedVehicle = Temp.GetComponentInParent<Vehicle>();
        Temp.AttachedVehicle.team = Team;
        Temp.EnemyBase = OpposingEnemy;
        Temp.SB.ObstacleAvoidanceOn();
        Temp.AttachedVehicle.EnemyBase = OpposingEnemy;
        Temp.FriendlyBase = this;
        Temp.GetComponent<Renderer>().material = Colour;
        Fighters.Add(Temp);
    }


    public override Vehicle Target(List<Vehicle> t)
    {
        throw new System.NotImplementedException();
    }


}
