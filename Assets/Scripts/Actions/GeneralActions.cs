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
            int remainingMovement = UGame.GetActiveCreature().GetRemainingMovement();
            int moveSpeed = UGame.GetActiveCreature().GetMovementSpeed();
            UGame.GetActiveCreature().SetReaminingMovement(remainingMovement + moveSpeed);
        }

        public static void Dodge(){
            UGame.GetActiveCreature().hasDisadvantageToBeHit = true;
        }

    }


        // Bonus Actions

        // Reactions
}
