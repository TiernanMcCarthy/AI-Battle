using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildState : State
{

    public override void Execute(StateObject s)
    {
        Homebase h = (Homebase)s;
        DesireabilityBrain Brain = new DesireabilityBrain();

        if(h.Miners.Count== h.MaxMiners)  //Put limits on whether these units can be built currently
        {
            h.BuildMiners = false;
        }
        else if(h.Miners.Count<h.MaxMiners)
        {
            h.BuildMiners = true;
        }

        if (h.Fighters.Count == h.MaxFighters)
        {
            h.BuildMiners = false;
        }
        else if (h.Fighters.Count < h.MaxFighters)
        {
            h.BuildMiners = true;
        }

        Brain.Check(h); //Based on the above conditions build units if the funds are available




            if (Brain.MinerDesire!=0 && Brain.MinerDesire > Brain.FighterDesire && h.BuildMiners==true) //Spawn the appropriate vehicle if it is currently desirable
            {
                h.SpawnMiner();
                h.StoredResources -= h.MinerCost;
            }
            else if(Brain.FighterDesire!=0)
            {
                h.SpawnFighter();
                h.StoredResources -= h.FighterCost;
            }




    }


}
