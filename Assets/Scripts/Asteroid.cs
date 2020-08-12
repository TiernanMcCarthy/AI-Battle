using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public int GoldCount; //Publicy extracted by miners

    public int Extract(int MineAmount)
    {
        if(MineAmount>GoldCount) //Return remaining Resource count
        {
            int Amount = GoldCount;
            GoldCount = 0;
            return Amount;
        }

        GoldCount -= MineAmount; //Minus intake
        //GoldCount++; //Regenerate one each time something is taken, to keep replenishing the supply
        return MineAmount; //Return the final sum

    }

}
