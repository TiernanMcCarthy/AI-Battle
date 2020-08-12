using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vehicle))]
public class SteeringBehaviours : MonoBehaviour {

    Vehicle vehicle;

    [Header("Seek")]
    public float SeekWeight = 1.0f;
    bool IsSeekOn = false;
    Vector3 SeekOnTargetPos;
    float SeekOnStopDistance;


    [Header("Wander")]
    public float WanderRadius = 10f;
    public float WanderDistance = 10f;
    public float WanderJitter = 1f;
    public float WanderWeight = 1.0f;
    bool IsWanderOn = false;
    Vector3 WanderTarget = Vector3.zero;



    [Header("Flee")]
    public float FleeDistance = 0;
    public float FleeWeight = 1.0f;
    bool IsFleeOn = false;
    Vector3 FleeFromTargetPos;

    [Header("Arrive")]
    public float ArriveSlowDownThreshold;
    public float ArriveSlowingDistance = 3.0f; //Adjust
    public float ArriveWeight = 1.0f;
    bool IsArriveOn = false;
    Vector3 ArriveTarget;

    [Header("Pursuit")]
    public Vehicle PursuitTarget;
    public float PursuitWeight;
    bool IsPursuitOn = false;

    [Header("Evade")]
    public Vehicle EvadeTarget;
    public float EvadeWeight = 1.0f;
    bool IsEvadeOn = false;

    [Header("Obstacle Avoidance")]
    public float AgentWidth; //Change with scale of object
    public float ObstacleAvoidanceWeight = 0.3f;
    public CollideCheck ObstacleChecker;
    bool IsObstacleAvoidanceOn = false;
    BoxCollider CollideManager;


    public bool IsMissile = false;

    private Vector3 RunningSum = Vector3.zero;

    private int Avoidance;
    // Use this for initialization
    void Start ()
    {
        vehicle = GetComponent<Vehicle>();
	}
	



    public void SetTeam(bool Team)
    {
        vehicle = GetComponentInParent<Vehicle>();
        vehicle.team = Team;
    }


    public bool AccumulateForce(Vector3 ForceToAdd)
    {
        float MagnitudeSoFar = RunningSum.magnitude;

        float MagnitudeRemaining = vehicle.MaxForce - MagnitudeSoFar;

        if(MagnitudeRemaining<=0.0f)
        {
            return false; //Cannot add anymore force
        }
        else if(ForceToAdd.magnitude<=MagnitudeRemaining) //If the force will not exceed the limit
        {
            RunningSum += ForceToAdd;
        }
        else
        {
            RunningSum += ForceToAdd.normalized * MagnitudeRemaining; //Add the remaining force in the direction of the original force.
        }
        return true;
    }

    public Vector3 Calculate()
    {
        Vector3 VelocitySum = Vector3.zero;
        RunningSum = Vector3.zero;
        bool ContinueAdding = true;
        Vector3 ForceToAdd = Vector3.zero;
            if (IsObstacleAvoidanceOn && ContinueAdding==true)
            {
                Avoidance++;
                if (Avoidance > 3)
                {
                 Avoidance = 0;
                }
                ForceToAdd = ObstacleAvoidance() * ObstacleAvoidanceWeight;
                ContinueAdding = AccumulateForce(ForceToAdd);

            }

            if (IsSeekOn && ContinueAdding == true)
            {
                if (Vector3.Distance(transform.position, SeekOnTargetPos) <= SeekOnStopDistance)
                {
                    //We're close enough to "stop"
                    IsSeekOn = false;

                    //Set the vehicle's velocity back to zero
                    vehicle.Velocity = Vector3.zero;

                    ContinueAdding = false;
                    RunningSum = Vector3.zero; // The vehicle should stop, at least for now considering it just floats on forever
                }
                else
                {
                    //VelocitySum += Seek(SeekOnTargetPos);
                    ForceToAdd = Seek(SeekOnTargetPos) * SeekWeight;
                    ContinueAdding = AccumulateForce(ForceToAdd);
                }
            }

            if (IsPursuitOn) //Pursuit just designates the seek target, this can be done without the weighting and cumulative force calculation
            {
                Pursuit(PursuitTarget);
                IsPursuitOn = false; 
            }


            if (IsEvadeOn && EvadeTarget!=null) //Evade just chooses a position to flee to, this doesn't need a check
            {
                Evade(EvadeTarget);
            if (Vector3.Distance(EvadeTarget.transform.position, transform.position) >= FleeDistance)
            {
                IsEvadeOn = false;
                IsFleeOn = false;
                Stop();
            }

            }
            //ObstacleAvoidance(ObstacleChecker.gameObject);
            if (IsFleeOn && ContinueAdding == true)
            {
                if (Vector3.Distance(transform.position, FleeFromTargetPos) >=FleeDistance)
                {
                    //We are far enough away to stop
                    IsFleeOn = false;

                    //Set the vehicle speed back to zero (Could make this vehicle dependent as a function saying "stop"
                    vehicle.Velocity = Vector3.zero;

                }
                else
                {
                    ForceToAdd = Flee(FleeFromTargetPos) * FleeWeight;
                    ContinueAdding = AccumulateForce(ForceToAdd);
          
                }
            }
            if (IsArriveOn && ContinueAdding == true)
            {
                if (Vector3.Distance(transform.position, ArriveTarget) <= ArriveSlowDownThreshold)
                {
                    transform.position = ArriveTarget; //perhaps?

                    vehicle.Velocity = Vector3.zero;

                    IsArriveOn = false;

                }
                else
                {
                    //vehicle.Velocity += Arrive(ArriveTarget);
                    ForceToAdd = Arrive(ArriveTarget);
                    ContinueAdding = AccumulateForce(ForceToAdd);
            }


            }
            if (IsWanderOn && ContinueAdding == true)
            {
                ForceToAdd = Wander()*WanderWeight;
                ContinueAdding = AccumulateForce(ForceToAdd);
            }
        //return VelocitySum;
        return RunningSum; //Return the combined sum of all the behaviours
    }


    public struct ID
    {
        public int Index;
        public Vector3 Position;

        public ID(int Inde,Vector3 Positi)
        {
            Index = Inde;
            Position = Positi;
        }
    }

    Vector3 ObstacleAvoidance()
    {


        Collider[] hitList = Physics.OverlapBox(transform.position + transform.forward * vehicle.Velocity.magnitude / 2, new Vector3(1, 1, vehicle.Velocity.magnitude), transform.rotation);


        List<ID> ClosestPositions = new List<ID>();

        Vector3 SteeringForce = Vector3.zero;
    
        if(hitList.Length!=0)
        {
            for(int i =0; i<hitList.Length; i++)
            {
                if(hitList[i].gameObject!=gameObject)
                {

      
                    //If this object is a missile, avoid friendly targets, otherwise ignore it
                    if (hitList[i].gameObject.GetComponent<Vehicle>() == true)
                    {

                        if(IsMissile==true && hitList[i].gameObject.GetComponentInParent<Vehicle>()==true)
                        {
                            Vehicle temp = hitList[i].gameObject.GetComponentInParent<Vehicle>();

                            if (temp.team == vehicle.team)
                            {
                                ID Close = new ID(i, transform.InverseTransformPoint(hitList[i].ClosestPoint(transform.position)));

                                ClosestPositions.Add(Close);
                            }
                        }
                        else
                        {
                            ID Close = new ID(i, transform.InverseTransformPoint(hitList[i].ClosestPoint(transform.position)));

                            ClosestPositions.Add(Close);
                        }
                    }
                    else
                    {
                        ID Close = new ID(i, transform.InverseTransformPoint(hitList[i].ClosestPoint(transform.position)));

                        ClosestPositions.Add(Close);
                    }
                }
            }
            if (ClosestPositions.Count != 0)
            {

                if (ClosestPositions.Count > 1)
                {
                    for (int i = 0; i < ClosestPositions.Count-1; i++) //Iterate through these and find the closest position by sorting
                    {

                        
                        if (Vector3.Distance(ClosestPositions[i].Position, transform.localPosition) > Vector3.Distance(ClosestPositions[i + 1].Position, transform.localPosition))
                        {
                            ID temp = ClosestPositions[i];
                            ClosestPositions[i] = ClosestPositions[i + 1]; //Swap these positions
                            ClosestPositions[i + 1] = temp;
                            i = -1; //Reiterate
                        }

                    }
                }

                float ForceMultiplierY = 1.0f + (vehicle.Velocity.magnitude - ClosestPositions[0].Position.x) / vehicle.Velocity.magnitude;


                float ForceMultiplierZ = 1.0f + (vehicle.Velocity.magnitude - ClosestPositions[0].Position.y) / vehicle.Velocity.magnitude;

                SteeringForce.y = (hitList[ClosestPositions[0].Index].bounds.extents.magnitude - ClosestPositions[0].Position.y) * (ForceMultiplierY - Random.Range(0,2));

    

                Vector3 FinalSum = transform.TransformDirection(SteeringForce);

                return FinalSum;
            }
        }



       

        

        return Vector3.zero;
    }

    Vector3 Obselete(GameObject Check)
    {




        Collider[] hitList=Physics.OverlapBox(transform.position + transform.forward * vehicle.Velocity.magnitude / 2, new Vector3(1, 1, vehicle.Velocity.magnitude), transform.rotation);

        ObstacleChecker.transform.localPosition = new Vector3(0, 0, vehicle.Velocity.magnitude / 2);
       




        Vector3 SteeringForce = new Vector3();
        List<ContactPoint> ContactList = new List<ContactPoint>();


        //Iterate through all appropriate colliders and then add the individual intersection points and dertermine the closest intersection point to the current agent
        for (int i=0; i<ObstacleChecker.CollideList.Count;i++)
        {
            for(int b=0; b<ObstacleChecker.CollideList[i].contactCount; b++)
            {
                ContactList.Add(ObstacleChecker.CollideList[i].contacts[b]);
            }

        }
        //Sort for the closest object
        if (ContactList.Count != 0)
        {
            bool iterate = true;
            while (iterate)
            {

                for (int i = 0; i < ContactList.Count - 1; i++)
                {

                    if (Vector3.Distance(ContactList[i].point, transform.position) > Vector3.Distance(ContactList[i + 1].point, transform.position))
                    {
                        ContactPoint temp = ContactList[i];
                        ContactList[i] = ContactList[i + 1];
                        ContactList[i + 1] = temp;
                        i = 0; //Reiterate
                    }

                }
                iterate = false;
            }



            GameObject AvoidObject = ContactList[0].thisCollider.gameObject; //Sorted closest object


            Matrix4x4 bob = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

            Matrix4x4 AvoidMatrix = Matrix4x4.TRS(AvoidObject.transform.position, AvoidObject.transform.rotation, AvoidObject.transform.localScale);

            Matrix4x4 LocalSpace = AvoidMatrix * Matrix4x4.Inverse(bob);



            Vector3 AvoidLocalSpace=LocalSpace.GetColumn(3);

            float ForceMultiplier = 1.0f + (vehicle.Velocity.magnitude - AvoidLocalSpace.x) / vehicle.Velocity.magnitude;

            SteeringForce.y = (AvoidObject.GetComponent<Collider>().bounds.extents.magnitude - AvoidLocalSpace.y) * ForceMultiplier;

            

        }

        return Vector3.zero;
    }

    Vector3 Seek(Vector3 TargetPos)
    {
        Vector3 DesiredVelocity = (TargetPos - transform.position).normalized * vehicle.MaxSpeed;

        return (DesiredVelocity - vehicle.Velocity);
    }

    Vector3 Flee(Vector3 TargetPos)
    {
        //GEt Desired Velocity
        Vector3 DesiredVelocity = (TargetPos- transform.position).normalized*vehicle.MaxSpeed;
        return (DesiredVelocity -vehicle.Velocity);
    }

    Vector3 Arrive(Vector3 Target)
    {
        Vector3 ToTarget = Target - transform.position;

        float Distance = ToTarget.magnitude;
        Vector3 DesiredVelocity = Vector3.zero;
        if (Distance>ArriveSlowDownThreshold)
        {
            float speed=Distance / ArriveSlowingDistance;

            Mathf.Clamp(speed, 0, vehicle.MaxSpeed);

             DesiredVelocity = ToTarget.normalized * speed / Distance;

        }

        return DesiredVelocity;
    }
    Vector3 Wander()
    {
        WanderTarget += new Vector3(
            Random.Range(-1f, 1f) * WanderJitter,
            0,
            Random.Range(-1f, 1f) * WanderJitter);

        WanderTarget.Normalize();

        WanderTarget *= WanderRadius;

        Vector3 targetLocal = WanderTarget;

        Vector3 targetWorld = transform.position + WanderTarget;

        targetWorld += transform.forward * WanderDistance;

        return targetWorld - transform.position;
    }

    void Pursuit(Vehicle Evader)
    {


        Vector3 ToEvader = Evader.transform.position;

        SeekOnTargetPos = Evader.transform.position; //Preset seek position to evader position incase the below statement is wrong.

        IsSeekOn = true;
        float RelativeHeading =Vector3.Dot(vehicle.Velocity.normalized, Evader.Velocity.normalized); //I believe

        if(RelativeHeading>=0) //If perpendicular or between? then pursue
        {
            float LookAheadTime=ToEvader.magnitude / (vehicle.MaxSpeed + Evader.Velocity.magnitude); //Magnitude gets the speed of the velocity 

            Vector3 EvaderFuturePosition = Evader.transform.position + Evader.Velocity * LookAheadTime;

            SeekOnTargetPos = EvaderFuturePosition;

        }

    }

    Vector3 Evade(Vehicle Pursuer)
    {
        Vector3 ToPursuer = Vector3.zero;
        if (Pursuer != null)
        {
            ToPursuer = Pursuer.transform.position - transform.position;
            float LookAheadTime;
            Vector3 PursuerFuturePosition;
            if (Pursuer.Velocity.magnitude != 0)
            {

                LookAheadTime = ToPursuer.magnitude / (vehicle.MaxSpeed + Pursuer.Velocity.magnitude); //Magnitude gets the speed of the velocity 
                PursuerFuturePosition = transform.position + Pursuer.Velocity * LookAheadTime;
            }
            else
            {
                LookAheadTime = ToPursuer.magnitude / (vehicle.MaxSpeed + Pursuer.MaxSpeed); //If the enemy isn't moving, still move fast
                PursuerFuturePosition = transform.position + Pursuer.transform.forward * LookAheadTime;
            }

            IsFleeOn = true;
            FleeFromTargetPos = PursuerFuturePosition;
        }
        return FleeFromTargetPos;
    }

    /// <summary>
    /// Will Seek to TargetPos until within StopDistance range from it
    /// </summary>
    /// <param name="TargetPos"></param>
    /// <param name="StopDistance"></param>
    public void SeekOn(Vector3 TargetPos, float StopDistance = 0.01f)
    {
        IsSeekOn = true;
        SeekOnTargetPos = TargetPos;
        SeekOnStopDistance = StopDistance;
    }

    public void SeekOff()
    {
        IsSeekOn = false;
    }

    public void FleeOn(Vector3 TargetPos,float DesiredDistance)
    {
        IsFleeOn = true;
        FleeDistance = DesiredDistance;
        FleeFromTargetPos = TargetPos;
    }

    public void FleeOff()
    {
        IsFleeOn = false;
    }

    public Vector3 EvadeOn(Vehicle Target)
    {
        EvadeTarget = Target;
        IsEvadeOn = true;
        return (Evade(Target));
    }
    public void Stop()
    {
        vehicle.Velocity = Vector3.zero;
    }
    public void EvadeOff()
    {
        IsEvadeOn = false;
    }

    public void PursuitOn(Vehicle Target)
    {
        IsPursuitOn = true;
    }
    public void PursuitOff()
    {
        IsPursuitOn = false;
    }

    public void WanderOn()
    {
        IsWanderOn = true;
    }

    public void WanderOff()
    {
        IsWanderOn = false;
        vehicle.Velocity = Vector3.zero;
    }

    public void ArriveOn(Vector3 Targetpos, float distance, float Slowdownthreshold)
    {
        IsArriveOn = true;
        ArriveTarget = Targetpos;
        ArriveSlowingDistance = distance;
        ArriveSlowDownThreshold = Slowdownthreshold;
    }

    public void ObstacleAvoidanceOn()
    {
        IsObstacleAvoidanceOn = true;
    }
    public void ObstacleAvoidanceOff()
    {
        IsObstacleAvoidanceOn = false;
    }

    public void ResetBehaviours()
    {
        IsArriveOn = false;
        IsEvadeOn = false;
        IsPursuitOn = false;
        IsSeekOn = false;
        IsFleeOn = false;
        IsWanderOn = false;
        vehicle.Velocity = Vector3.zero;
    }

    void OnDrawGizmos()
    {

        

        
    }
}
