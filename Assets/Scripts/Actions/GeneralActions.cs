using System;
using System.Reflection;
using System.Collections.Generic;
using GameUtils;
using UnityEngine;

public class GeneralActions 
{

    public static List<System.Action> GetGeneralActions(){
        return new List<System.Action>() { 
            Actions.Dash,
            Actions.Dodge
        };
    }

    public static List<string> GetNames(){
        List<string> names = new();
        foreach (MethodInfo method in typeof(Actions).GetMethods()){
            names.Add(method.Name);
        }
        return names;
    }

    public static class Actions {

        // Actions
        public static void Dash(){
            int remainingMovement = UGame.GetActiveCreatureStats().GetRemainingMovement();
            int moveSpeed = UGame.GetActiveCreatureStats().GetMovementSpeed();
            UGame.GetActiveCreatureStats().SetReaminingMovement(remainingMovement + moveSpeed);
        }

        public static void Dodge(){

        }

    }


        // Bonus Actions

        // Reactions
}
