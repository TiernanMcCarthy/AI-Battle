using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterEvaluate : State
{
    struct Largest
    {
        public int Index;
        public float Size;
        public Largest(int ind,float s)
        {
            Index = ind;
            Size = s;
        }



    }
    public override void Execute(StateObject s)
    {
        DesireabilityBrain Brain = new DesireabilityBrain();
        
        Fighter F = (Fighter)s;
        F.AttachedVehicle.Scan(); //Look for enemys within the ship's scan range

        Brain.Set(F.AttachedVehicle, F.Enemy, F.FriendlyBase.BaseObject); //Assign these values to the brain

        List<Largest> Value = new List<Largest>();


        Brain.Check(); //Check what current state is most suitable considering the ship's surroundings

        Value.Add(new Largest(0, Brain.GetAmmoDesire));
        Value.Add(new Largest(1, Brain.GetPowerfulness));
        Value.Add(new Largest(2, Brain.GetHealthDesire));
        bool change = false; 

        for (int i = 0; i < Value.Count - 1; i++) //Work out which state is most ideal
        {
            if (Value[i].Size < Value[i + 1].Size)
            {
                Largest temp = Value[i + 1];
                Value[i + 1] = Value[i];
                Value[i] = temp;
                change = true;
                i = -1; //Reset
            }
        }


        if (change == true)
        {
            switch (Value[0].Index) //Execute the appropriate state
            {
                case 0: //Ammo
                    if (F.CanIHeal() == true )
                    {
                        F.CurrentState = new ReturnHome();
                        break;
                    }
                    else
                    {
                        F.CurrentState = new Attack();
                        break;
                    }
                case 2: //Heal
                    if (F.CanIHeal() == true)
                    {
                        F.CurrentState = new ReturnHome();
                        break;
                    }
                    else
                    {
                        F.CurrentState = new Attack();
                        break;
                    }
                case 1: //Powerfulness
                    F.CurrentState = new Attack();
                    break;


            }
        }



        if (F.Enemy == null && F.EnemyBase.BaseObject!=null) //If no target has been found, attack the base object
        {
            
            F.Enemy = F.EnemyBase.BaseObject;
            F.AttachedVehicle.TargetVehicle = F.EnemyBase.BaseObject;

            
        }

    }
}
