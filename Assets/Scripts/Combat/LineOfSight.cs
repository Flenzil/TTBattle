using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtils;
using Unity.VisualScripting;
using System;
using UnityEngine.UIElements;

public static class LineOfSight {

    public static Creature[] creaturesInScene = FindCreaturesInScene();

    public static List<Creature> CreaturesWithLineOfSight(){

        List<Creature> creaturesWithLineOfSight = new();
        foreach (Creature creature in creaturesInScene){
            if (creature == UGame.GetActiveCreature()){
                continue;
            }
            if (CanSeeActiveCreature(creature)){
                Debug.Log("Seen by" + creature);
                creaturesWithLineOfSight.Add(creature);
            }
        }
        return creaturesWithLineOfSight;
    }

    public static List<Creature> CreaturesWithinLineOfSight(){

        // Right now this is not too different to creautres with line of sight.
        // The only difference is that this returns an empty list if the active
        // creature is blinded.
        // In the future however, creatures with line of sight and creatures withIN
        // line of sight may be different when things like different vision types
        // are implemented e.g being in darkness and an enemy has darkvision but the 
        // active creature does not.

        List<Creature> creaturesWithinLineOfSight = new();

        if (UGame.GetActiveCreature().currentConditions.Contains(Condition.blinded)){
            return creaturesWithinLineOfSight;
        }

        foreach (Creature creature in creaturesInScene){
            if (creature == UGame.GetActiveCreature()){
                continue;
            }
            if (CanSeeActiveCreature(creature)){
                Debug.Log("Can see" + creature);
                creaturesWithinLineOfSight.Add(creature);
            }
        }
        return creaturesWithinLineOfSight;

    }

    private static Creature[] FindCreaturesInScene(){
        return UnityEngine.Object.FindObjectsByType<Creature>(FindObjectsSortMode.None);
    }

    private static bool CanSeeActiveCreature(Creature creature){

        // Line of sight is detirmined by drawing a line between the corners of the 
        // current creature to the corners of the target creature. If any of those
        // lines are unobstructed, then line of sight is established.
        if (creature.currentConditions.Contains(Condition.blinded)){
            return false;
        }

        List<Vector3> activeCreatureCorners = UGame.GetActiveCreature().GetCorners();
        List<Vector3> creatureCorners = creature.GetCorners();
        
        foreach (Vector3 activeCorner in activeCreatureCorners) {
            foreach (Vector3 corner in creatureCorners){
                Vector3 rayDirection = corner - activeCorner;

                Debug.DrawRay(activeCorner + Vector3.up * 0.1f, rayDirection + Vector3.up * 0.1f, Color.red, 5f);

                if (!Physics.Raycast(
                        origin: activeCorner,
                        direction: rayDirection, 
                        maxDistance : Vector3.Distance(activeCorner, corner),
                        layerMask : ~(1 << LayerMask.NameToLayer("Creatures"))
                    ) 
                ){
                    return true;
                }
            }
        }
        return false;
    }

}
