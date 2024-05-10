using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CreatureUtils;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

namespace PathingUtils {

    public static class UPathing {

        public static Vector3 XZPlane(float x, float z) {
            return new Vector3(x, 0, z);
        }
    
        public static void GetSeekRadius(CreatureSize creatureSize, out int seekRadiusStart, out int seekRadiusEnd){
            switch(creatureSize){
                case CreatureSize.large:
                    seekRadiusStart = 0;
                    seekRadiusEnd = 1;
                    break;
                case CreatureSize.huge:
                    seekRadiusStart = -1;
                    seekRadiusEnd = 1;
                    break;
                case CreatureSize.gargantuan:
                    seekRadiusStart = -1;
                    seekRadiusEnd = 2;
                    break;
                default:
                    seekRadiusStart = 0;
                    seekRadiusEnd = 0;
                    break;
            }
        }

        public static Vector3 GetPosition(GameObject creature){
            return creature.transform.GetChild(0).position;
        }


        public static bool IsOccupiedByEnemy(PathNode node, GameObject activeCreature){
            if (node.isOccupied && !activeCreature.CompareTag(node.GetOccupyingCreature().tag)) return true;
            return false;
        }


        public static void ApplyFuncToCreatureSpace(GameObject creature, Action<int, int> func){

            CreatureSize creatureSize = creature.GetComponent<CreatureStats>().GetSize();
            Pathfinding.GetGrid().GetXY(creature.transform.position, out int x, out int y);
            GetSeekRadius(creatureSize, out int seekRadiusStart, out int seekRadiusEnd);

            for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
                for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                    func(x + i, y + j);
                }
            }
        }

        public static void ApplyFuncToCreatureSpace(GameObject creature, int x, int y, Action<int, int> func){

            CreatureSize creatureSize = creature.GetComponent<CreatureStats>().GetSize();
            GetSeekRadius(creatureSize, out int seekRadiusStart, out int seekRadiusEnd);

            for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
                for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                    func(x + i, y + j);
                }
            }
        }

        public static void SetCreatureSpaceToOccupied(GameObject creature){
            ApplyFuncToCreatureSpace(creature, (a, b) => {
                Pathfinding.GetGrid().GetGridObject(a,b).SetOccupyingCreature(creature);
            });
        }

        public static void SetCreatureSpaceToOccupied(GameObject creature, int x, int y){
            ApplyFuncToCreatureSpace(creature, x, y, (a, b) => {
                Pathfinding.GetGrid().GetGridObject(a,b).SetOccupyingCreature(creature);
            });
        }

        public static void SetCreatureSpaceToUnoccupied(GameObject creature){
            Pathfinding.GetGrid().GetXY(creature.transform.GetChild(0).position, out int x, out int y);
            SetCreatureSpaceToUnoccupied(creature, x, y);
        }

        public static void SetCreatureSpaceToUnoccupied(GameObject creature, int x, int y){
            ApplyFuncToCreatureSpace(creature, x, y, (a, b) => {
                Pathfinding.GetGrid().GetGridObject(a,b).ClearOccupyingCreature();
            });
        }

    }
}

