using System.Collections;
using System.Collections.Generic;
using CreatureUtils;
using UnityEngine;

namespace GameUtils {
    public static class UGame {
        
        public static Vector3 GetMousePosition3D(Camera mainCamera) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit raycastHit);
            return raycastHit.point;
        }

        public static Vector3 GetMousePosition3D(Camera mainCamera, string layer) {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, LayerMask.GetMask(layer))) {
                return raycastHit.point;
            } else {
                return Vector3.zero;
            };
        }

        public static GameObject GetActiveCreature(){
            return GameManager.Instance.activePlayer;
        }

        public static CreatureStats GetActiveCreatureStats(){
            return GetActiveCreature().GetComponent<CreatureStats>();
        }

        public static CreatureStats GetCreatureStats(GameObject creature){
            return creature.GetComponent<CreatureStats>();
        }

        public static void SetActiveCreature(GameObject creature){
            GameManager.Instance.activePlayer = null;
            GameManager.Instance.activePlayer = creature;
        }
        
        public static CreatureSize GetActiveCreatureSize(){
            return GetActiveCreature().GetComponent<CreatureStats>().GetSize();
        }
    }
}