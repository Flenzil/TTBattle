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

        public static Creature GetActiveCreature(){
            return GameManager.Instance.activeCreature;
        }

        public static Actions GetActiveCreatureActions(){
            return GetActiveCreature().GetComponent<Actions>();
        }

        public static Health GetActiveCreatureHealth(){
            return GetActiveCreature().GetComponent<Health>();
        }

        public static Attack GetActiveAttack(){
            return GetActiveCreature().GetComponent<Actions>().GetActiveAttack();
        }

        public static Vector3 GetActiveCreaturePosition(){
            if (GetActiveCreature().transform.childCount == 0){
                return GetActiveCreature().transform.position;
            } else {
                return GetActiveCreature().transform.GetChild(0).transform.position;
            }
        }

        public static void SetActiveCreature(Creature creature){
            GameManager.Instance.activeCreature = null;
            GameManager.Instance.activeCreature = creature;
        }
        
        public static CreatureSize GetActiveCreatureSize(){
            return GetActiveCreature().GetSize();
        }
    }
}