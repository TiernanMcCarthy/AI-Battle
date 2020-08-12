using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class StateObject :MonoBehaviour //abstract class for executing states
{
    public Vehicle TargetObject;
    public bool Team; //red is false, blue is true
    public abstract Vehicle Target(List<Vehicle> t);

    public abstract void SetParentTarget(Vehicle t); 
}
