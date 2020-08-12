using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteeringBehaviours))]
public class Vehicle :MonoBehaviour { 

    /////////////////////
    //Updated Values
    /////////////////////
    /// <summary>
    /// This is applied to the current position every frame
    /// </summary>
    public Vector3 Velocity;

    //Position, Heading and Side can be accessed from the transform component with transform.position, transform.forward and transform.right respectively

    //"Constant" values, they are public so we can adjust them through the editor

    //Represents the weight of an object, will effect its acceleration
    public float Mass = 1;

    //The maximum speed this agent can move per second
    public float MaxSpeed = 1;

    //The thrust this agent can produce
    public float MaxForce = 1;

    //We use this to determine how fast the agent can turn, but just ignore it for, we won't be using it
    public float MaxTurnRate = 1.0f;


    //Vehicles scan for targets, this should happen within their sensor range and at their set interval
    public float ScanRange = 5.0f;

    public float ScanInterval = 3.0f; //Once every 3 seconds

    public Vehicle TargetVehicle;

    public bool team; //Red is false blue is true

    public StateObject Parent;
    private SteeringBehaviours SB;

    public Homebase EnemyBase;

    public float Health = 100; //100 default
    public float MaximumHealth;

    public float Ammo = 20;

    public float MaximumAmmo = 20;

    // Use this for initialization
    void Start ()
    {
        MaximumHealth = Health;
        Parent = GetComponentInParent<StateObject>();
        SB = GetComponent<SteeringBehaviours>();
        gameObject.tag = "Vehicle";
	}
	
	// Update is called once per frame
	void Update ()
    {

        if(Health<=0)
        {
            Destroy(gameObject);
        }

        Vector3 SteeringForce = SB.Calculate();

        Vector3 Acceleration = SteeringForce / Mass;

        Velocity += Acceleration;

        Velocity = Vector3.ClampMagnitude(Velocity, MaxSpeed);

        if (Velocity != Vector3.zero)
        {
            transform.position += Velocity * Time.deltaTime;

            transform.forward = Velocity.normalized;
        }

        //transform.right should update on its own once we update the transform.forward
	}

    public void SetVehicle(GameObject v)
    {
        TargetVehicle = v.GetComponentInParent<Vehicle>();
        Parent.SetParentTarget(v.GetComponentInParent<Vehicle>());
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<Projectile>())
        {
            if (GetComponentInParent<Projectile>() != true)
            {
                Projectile temp = collision.gameObject.GetComponent<Projectile>();

                Health -= temp.Damage;
                Destroy(collision.gameObject);
                SB.ResetBehaviours();
            }
        }

    }

    public void Scan()
    {

        Collider[] hitList = Physics.OverlapSphere(transform.position, ScanRange);
        List<SteeringBehaviours.ID> ClosestPositions = new List<SteeringBehaviours.ID>();
        if (hitList.Length > 1)
        {
            for (int i = 0; i < hitList.Length; i++)
            {
                if (hitList[i].gameObject != gameObject && hitList[i].gameObject.tag == "Vehicle" && hitList[i].gameObject.GetComponent<Projectile>()!=true)
                {

                    if (hitList[i].gameObject.GetComponentInParent<Vehicle>().team != team)
                    {
                        // SteeringBehaviours.ID Close = new SteeringBehaviours.ID(i, transform.position);
                        SteeringBehaviours.ID Close = new SteeringBehaviours.ID(i, hitList[i].transform.position);
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


                        if (Vector3.Distance(ClosestPositions[i].Position, transform.position) > Vector3.Distance(ClosestPositions[i + 1].Position, transform.position))
                        {
                            SteeringBehaviours.ID temp = ClosestPositions[i];
                            ClosestPositions[i] = ClosestPositions[i + 1]; //Swap these positions
                            ClosestPositions[i + 1] = temp;
                            i = -1; //Reiterate
                        }

                    }
                }
                SetVehicle(hitList[ClosestPositions[0].Index].gameObject); //set the target vehicle
            }

        }


    }
}
