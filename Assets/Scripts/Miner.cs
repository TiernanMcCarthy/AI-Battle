using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[RequireComponent(typeof(SteeringBehaviours))]
[RequireComponent(typeof(MinerBB))]
public class Miner :StateObject
{

    public int ResourceUnit = 0; // Collectable resource from asteroids
    public int MineStrength = 2;
    public int MaximumResourceStorage;
    public float MineSpeed = 2.0f; //Seconds



    public State CurrentState;

    public SteeringBehaviours SB;

    public Asteroid MiningTarget;

    public Vehicle AttachedVehicle;


    public Vehicle FleeTarget;

    public Homebase HQ;

    // Start is called before the first frame update

    private BTNode BTRootNode;
    private MinerBB mBB;
    void Initialise()
    {
        CurrentState = new ScanState();

        AttachedVehicle = GetComponentInChildren<Vehicle>();
        mBB = GetComponentInParent<MinerBB>();
        //Create a reference to a Blackboard for a miner
        mBB.HQ = HQ;
        mBB.MiningTarget = MiningTarget;

        mBB.depositing = false;

        SB = GetComponent<SteeringBehaviours>();

        SB.ObstacleAvoidanceOn();

        //If a vehicle is spawned with predefined behaviour tree blackboard attributes, they should be applied
        if(MiningTarget!=null)
        {
            mBB.MiningTarget = MiningTarget;
        }

        if (FleeTarget != null)
        {
            mBB.FleeTarget = FleeTarget;
        }


        AttachedVehicle.team = Team;


        //Create the root selector for the tree
        Selector rootChild = new Selector(mBB);
        BTRootNode = rootChild;

        //Create Flee Sequence
        CompositeNode fleeSequence = new Sequence(mBB);
        MinerFleeDecorator fleeRoot = new MinerFleeDecorator(fleeSequence, mBB,this);
        fleeSequence.AddChild(new MinerCalculateFleeLocation(mBB,this));
        fleeSequence.AddChild(new MinerMoveTo(mBB, this));
        fleeSequence.AddChild(new MinerWaitTillAtLocation(mBB, this));
        fleeSequence.AddChild(new DelayNode(mBB, 2.0f));

        CompositeNode mining = new Sequence(mBB);

        MineSequence MiningRoot = new MineSequence(mining, mBB, this);
        mining.AddChild(new MoveToAsteroid(mBB, this));
        mining.AddChild(new CollectResources(mBB, this));
        mining.AddChild(new ReturnToBase(mBB, this));
        mining.AddChild(new DelayNode(mBB, 2.0f));


        rootChild.AddChild(MiningRoot);
        rootChild.AddChild(fleeRoot);






        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);


        mBB.FleeTarget = FleeTarget;
    }

    public override void SetParentTarget(Vehicle t)
    {
        
    }


    public void InstansiateStart()
    {
        Initialise();
    }
    public void Start()
    {
        Initialise();
    }
    public override Vehicle Target(List<Vehicle> t) //Get target from object list
    {
        throw new System.NotImplementedException();
    }



    public bool CheckDistanceFromTarget()
    {
        if (mBB.FleeTarget != null)
        {
           // Debug.Log(Vector3.Distance(transform.position, mBB.FleeTarget.transform.position));
        }
        if (mBB.FleeTarget != null && Vector3.Distance(transform.position, mBB.FleeTarget.transform.position) <= mBB.FleeProximity)
        {
            mBB.fleeing = true;

            return true;
        }
        mBB.fleeing = false;
        return false;
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }


    #region Mine BehaviourTree

    public class MineSequence : ConditionalDecorator
    {
        MinerBB mBB;
        Miner MinerRef;
        public MineSequence(BTNode WrappedNode, Blackboard bb, Miner mine) : base(WrappedNode, bb)
        {
            mBB = (MinerBB)bb;
            MinerRef = mine;

            mBB.FleeTarget = MinerRef.AttachedVehicle.TargetVehicle;
        }


        public override bool CheckStatus()
        {

            if (mBB != null)
            {
                mBB.MiningTarget = MinerRef.MiningTarget;
                if (mBB.MiningTarget != null && mBB.fleeing == false) //If an asteroid has been discovered and the ship isn't being chased, mine
                {
                    return true;
                }
                else
                {
                   // MinerRef.SB.ResetBehaviours(); //Reset all behaviours except obstacle avoidance so that the ship stops its current behaviour
                }

            }

            if(MinerRef.CheckDistanceFromTarget() == true)
            {
                mBB.fleeing = true;

                return false; 

            }
            else
            {
                mBB.fleeing = false;
                MinerRef.SB.ResetBehaviours();
            }

            return false;
        }

    }

    #endregion


    public class MoveToAsteroid :BTNode
    {

        private MinerBB mBB;
        private Miner MinerRef;
        public MoveToAsteroid(Blackboard bb, Miner mine) : base(bb)
        {
            mBB = (MinerBB)bb;
            MinerRef = mine;
        }

        public override BTStatus Execute()
        {
          
                MinerRef.SB.ArriveOn(mBB.MiningTarget.transform.position+new Vector3(Random.Range(-2,2), Random.Range(-2, 2), Random.Range(-2, 2)), 2.9f, 0.1f);
          

            if (Vector3.Distance(MinerRef.transform.position,mBB.MiningTarget.transform.position)<=5) //Wait until the ship has arrived
            {
                MinerRef.SB.ResetBehaviours();

                return BTStatus.SUCCESS;
            }
            else if(mBB.depositing==true)
            {
                return BTStatus.SUCCESS;
            }

            if (MinerRef.CheckDistanceFromTarget() == true)
            {
               // mBB.fleeing = true;
                
                return BTStatus.FAILURE;

            }
            else
            {
                mBB.fleeing = false;
            }

            //  else if(mBB.fleeing==true) //Make the Miner flee
            //   {
            //    Debug.Log("FLEEING");
            //    return BTStatus.FAILURE;
            // }
            return BTStatus.RUNNING;
        }
    }


    public class CollectResources :BTNode
    {

        private MinerBB mBB;
        private Miner MinerRef;
        private float lastmine;

        public CollectResources(Blackboard bb, Miner mine) : base(bb)
        {
            mBB = (MinerBB)bb;
            MinerRef = mine;
            lastmine = Time.time;
        }

        public override BTStatus Execute()
        {
            if (Time.time - lastmine >= MinerRef.MineSpeed) //Only mine as fast as the desired speed
            {

                lastmine = Time.time;
                if (Vector3.Distance(mBB.MiningTarget.transform.position, MinerRef.transform.position) < 5.0f) //Mine from the asteroid
                {
                    if (mBB.MiningTarget.GoldCount != 0)
                    {
                        MinerRef.ResourceUnit += mBB.MiningTarget.Extract(MinerRef.MineStrength);
                    }


                }
            }

            if (MinerRef.CheckDistanceFromTarget()==true)
            {
               // mBB.fleeing = true;

                return BTStatus.FAILURE;

            }
            else
            {
                mBB.fleeing = false;
            }


            if (MinerRef.ResourceUnit>= MinerRef.MaximumResourceStorage || mBB.MiningTarget.GoldCount==0 && mBB.HQ!=null)
            {
                if (mBB.HQ != null)
                {
                    MinerRef.SB.ArriveOn(mBB.HQ.transform.position + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-2, 2)), 2.9f, 0.1f);
                }
                return BTStatus.SUCCESS;

            }

            

            return BTStatus.RUNNING;


        }

    }

    public class ReturnToBase : BTNode
    {
        private MinerBB mBB;
        private Miner MinerRef;

        public ReturnToBase(Blackboard bb, Miner mine) : base(bb)
        {
            mBB = (MinerBB)bb;
            MinerRef = mine;
        }

        public override BTStatus Execute()
        {
            mBB.depositing = true;
            if (mBB.HQ != null)
            {
                if (Vector3.Distance(MinerRef.transform.position, mBB.HQ.transform.position) < 5.0f)
                {
                    mBB.HQ.Deposit(MinerRef.ResourceUnit);
                    MinerRef.SB.ResetBehaviours();
                    MinerRef.ResourceUnit = 0;
                    mBB.depositing = false;
                    return BTStatus.FAILURE;

                }
            }
            return BTStatus.RUNNING;

        }
    }

    #region Flee BehaviourTree

    public class MinerCalculateFleeLocation : BTNode
    {
        private MinerBB mBB;
        private Miner MinerRef;
        public MinerCalculateFleeLocation(Blackboard bb,Miner mine) : base(bb)
        {
            mBB = (MinerBB)bb;
            MinerRef=mine;
        }

        public override BTStatus Execute()
        {

            mBB.MoveToLocation = MinerRef.SB.EvadeOn(mBB.FleeTarget);
            return BTStatus.SUCCESS;
        }
    }

    public class MinerMoveTo : BTNode
    {
        private MinerBB mBB;
        private Miner MinerRef;

        public MinerMoveTo(Blackboard bb, Miner mine) : base(bb)
        {
            mBB = (MinerBB)bb;
            MinerRef = mine;
        }

        public override BTStatus Execute()
        {
            MinerRef.SB.EvadeOn(mBB.FleeTarget); //Perhaps make it evade
            MinerRef.SB.FleeDistance = mBB.FleeProximity;
            return BTStatus.SUCCESS;
        }
    }

    public class MinerFleeDecorator : ConditionalDecorator
    {
        MinerBB mBB;
        Miner MinerRef;
        public MinerFleeDecorator(BTNode WrappedNode, Blackboard bb,Miner mine) : base(WrappedNode, bb)
        {
            mBB = (MinerBB)bb;
            MinerRef = mine;
        }


        public override bool CheckStatus()
        {
            if (mBB != null && mBB.FleeTarget!=null) //Flee if target is within reach
            {


                float Distance = Vector3.Distance(mBB.FleeTarget.transform.position, MinerRef.transform.position);


                if(mBB.FleeProximity>=Vector3.Distance(mBB.FleeTarget.transform.position, MinerRef.transform.position))
                {
                    mBB.fleeing = true;
                    return true;
                }


            }

            return false;
        }



    }

    public class MinerWaitTillAtLocation :BTNode
    {
        private MinerBB mBB;
        private Miner MinerRef;

        public MinerWaitTillAtLocation(Blackboard bb, Miner Mine): base(bb)
        {
            mBB = (MinerBB)bb;
            MinerRef = Mine;
        }

        public override BTStatus Execute()
        {
            BTStatus rv = BTStatus.RUNNING;


           if(Vector3.Distance(mBB.FleeTarget.transform.position,MinerRef.transform.position)>mBB.FleeProximity-1)
            {
                rv = BTStatus.SUCCESS;
                MinerRef.SB.EvadeOff();
                MinerRef.SB.FleeOff();
                MinerRef.SB.Stop();
                mBB.fleeing = false;
            }

            return rv;
        }



    }


    #endregion

    // Update is called once per frame
    void Update()
    {
        if(CurrentState==null)
        {
            CurrentState = new ScanState();
        }
        CurrentState.Execute(this);
        mBB.FleeTarget = AttachedVehicle.TargetVehicle;
        FleeTarget = AttachedVehicle.TargetVehicle;

        if(mBB.MiningTarget.GoldCount==0)
        {
            HQ.Deposit(HQ.MinerCost); // Deposit Value of Miner
            Destroy(gameObject);
        }
    }


    private void FixedUpdate()
    {
        
    }

}

