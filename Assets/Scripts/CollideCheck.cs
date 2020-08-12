using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideCheck : MonoBehaviour
{
    public List<Collision> CollideList;

    public void Start()
    {
        CollideList = new List<Collision>();
    }


    private void OnTriggerEnter(Collider other)
    {
        //other.
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(CollideList.Contains(collision)!=true)
        {
            CollideList.Add(collision);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(CollideList.Contains(collision))
        {
            CollideList.Remove(collision);
        }
    }
}
