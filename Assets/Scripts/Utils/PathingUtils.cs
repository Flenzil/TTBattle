using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CreatureUtils;
using NUnit.Framework;
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


        public static bool IsOccupiedByEnemy(PathNode node, GameObject activeCreature){
            if (node.isOccupied && !activeCreature.CompareTag(node.GetOccupyingCreature().tag)) return true;
            return false;
        }


        public static void ApplyFuncToCreatureSpace(GameObject creature, Grid<PathNode> grid, Action<int, int> func){

            CreatureSize creatureSize = creature.GetComponent<CreatureStats>().GetSize();
            grid.GetXY(creature.transform.position, out int x, out int y);
            GetSeekRadius(creatureSize, out int seekRadiusStart, out int seekRadiusEnd);

            for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
                for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                    func(x + i, y + j);
                }
            }
        }

        public static void ApplyFuncToCreatureSpace(GameObject creature, Grid<PathNode> grid, int x, int y, Action<int, int> func){

            CreatureSize creatureSize = creature.GetComponent<CreatureStats>().GetSize();
            GetSeekRadius(creatureSize, out int seekRadiusStart, out int seekRadiusEnd);

            for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
                for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                    func(x + i, y + j);
                }
            }
        }

        public static void SetCreatureSpaceToOccupied(GameObject creature, Grid<PathNode> grid){
            ApplyFuncToCreatureSpace(creature, grid, (a, b) => grid.GetGridObject(a,b).SetOccupyingCreature(creature));
        }

        public static void SetCreatureSpaceToOccupied(GameObject creature, Grid<PathNode> grid, int x, int y){
            ApplyFuncToCreatureSpace(creature, grid, x, y, (a, b) => grid.GetGridObject(a,b).SetOccupyingCreature(creature));
        }

        public static void SetCreatureSpaceToUnoccupied(GameObject creature, Grid<PathNode> grid){
            ApplyFuncToCreatureSpace(creature, grid, (a, b) => grid.GetGridObject(a,b).ClearOccupyingCreature());
        }

        public static void SetCreatureSpaceToUnoccupied(GameObject creature, Grid<PathNode> grid, int x, int y){
            ApplyFuncToCreatureSpace(creature, grid, x, y, (a, b) => grid.GetGridObject(a,b).ClearOccupyingCreature());
        }

    }
}

