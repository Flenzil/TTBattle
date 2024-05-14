using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreatureUtils;
using CombatUtils;
using System;
using System.Linq;
using UnityEngine.Rendering;
using System.Diagnostics.CodeAnalysis;
using UnityEditorInternal;
using PathingUtils;
using Unity.Burst.Intrinsics;

public class Creature : MonoBehaviour
{
    [SerializeField] SOCreature stats;

    public bool hasAdvantageToHit = false;
    public bool hasDisadvantageToHit = false;
    public bool hasAdvantageToBeHit = false;
    public bool hasDisadvantageToBeHit = false;
    public List<Condition> currentConditions = new();
    public List<Creature> seenBy;
    public List<Creature> canSee;


    private List<PathNode> occupiedNodes = new();

    private int remainingMovement;
    public int GetMaxHP(){ return stats.maxHP;}
    public int GetAC(){ return stats.ac;}
    public int GetMovementSpeed(){ return stats.moveSpeed;}
    public CreatureSize GetSize(){ return stats.size;}
    public int GetLevel(){ return stats.level;}

    void Start(){
        remainingMovement = GetMovementSpeed();
    }
    

    public Vector3 GetPosition(){
        // The centre of a creature is determined by an anchor GameObject which is parented to the creature.
        return transform.GetChild(0).position;
    }

    public List<Vector3> GetCorners(){
        // Returns the position of the four corners of a creature's occupied space

        // If a creature is next to a wall, a raycast originating from its corner,
        // may start from within the wall and thus won't collide with it, so the
        // positions of the corner are moved slightly inside the creature's space
        float tinyOffset = 0.01f; 

        List<Vector3> allCorners = new();
        foreach (PathNode node in occupiedNodes){
            allCorners.Add(new Vector3(node.x, 0, node.y));
            allCorners.Add(new Vector3(node.x + 1f, 0, node.y));
            allCorners.Add(new Vector3(node.x, 0, node.y + 1f));
            allCorners.Add(new Vector3(node.x + 1f, 0, node.y + 1f));
        }

        List<Vector3> fourCorners = new() {
            new Vector3(allCorners.Max(v => v.x) - tinyOffset, 0, allCorners.Max(v => v.z) - tinyOffset),
            new Vector3(allCorners.Max(v => v.x) - tinyOffset, 0, allCorners.Min(v => v.z) + tinyOffset),
            new Vector3(allCorners.Min(v => v.x) + tinyOffset, 0, allCorners.Max(v => v.z) - tinyOffset),
            new Vector3(allCorners.Min(v => v.x) + tinyOffset, 0, allCorners.Min(v => v.z) + tinyOffset)
        };

        return fourCorners;
    }

    public void GetSeekRadius(out int seekRadiusStart, out int seekRadiusEnd){
        switch(GetSize()){
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

    public List<PathNode> GetOccupiedNodes(){
        return occupiedNodes;
    }

    public void SetOccupiedNodes(List<PathNode> pathNodes) {
        occupiedNodes = pathNodes;
    }

    public List<PathNode> GetOccupiedNodes(PathNode node){
        // Returns nodes that a creature would occupy if it were positioned at
        // node. Useful for pathfinding.
        GetSeekRadius(out int seekRadiusStart, out int seekRadiusEnd);
        List<PathNode> nodes = new();

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                PathNode currentNode = Pathfinding.GetGrid().GetGridObject(node.x + i, node.y + j);
                if (currentNode != null){
                    nodes.Add(currentNode);
                }
            }
        }
        return nodes;
    }

    public void ApplyFuncToCreatureSpace(Action<int, int> func){

        GetSeekRadius(out int seekRadiusStart, out int seekRadiusEnd);
        Pathfinding.GetGrid().GetXY(GetPosition(), out int x, out int y);

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                func(x + i, y + j);
            }
        }
    }

    public void ApplyFuncToCreatureSpace(int x, int y, Action<int, int> func){

        GetSeekRadius(out int seekRadiusStart, out int seekRadiusEnd);

        for (int i = seekRadiusStart; i <= seekRadiusEnd; i++){
            for (int j = seekRadiusStart; j <= seekRadiusEnd; j++){
                func(x + i, y + j);
            }
        }
    }

    public void SetCreatureSpaceToOccupied(){
        occupiedNodes.Clear();
        ApplyFuncToCreatureSpace((a, b) => {
            Pathfinding.GetGrid().GetGridObject(a,b).SetOccupyingCreature(this);
            occupiedNodes.Add(Pathfinding.GetGrid().GetGridObject(a,b));
        });
    }

    public void SetCreatureSpaceToOccupied(int x, int y){
        occupiedNodes.Clear();
        ApplyFuncToCreatureSpace(x, y, (a, b) => {
            Pathfinding.GetGrid().GetGridObject(a,b).SetOccupyingCreature(this);
            occupiedNodes.Add(Pathfinding.GetGrid().GetGridObject(a,b));
        });
    }

    public void SetCreatureSpaceToUnoccupied(){
        Pathfinding.GetGrid().GetXY(GetPosition(), out int x, out int y);
        SetCreatureSpaceToUnoccupied(x, y);
    }

    public void SetCreatureSpaceToUnoccupied(int x, int y){
        ApplyFuncToCreatureSpace(x, y, (a, b) => {
            Pathfinding.GetGrid().GetGridObject(a,b).ClearOccupyingCreature();
        });
    }

    public int GetRemainingMovement(){
        return remainingMovement;
    }

    public void SetReaminingMovement(int movement){
        remainingMovement = movement;
    }


    public void DecreaseRemainingMovement(int decrease){
        remainingMovement -= decrease;
    }


    public List<WeaponType> GetWeaponProficiencies(){
        List<WeaponType> proficientWeapons = new();
        
        if (stats.weaponCatagories != null){
            if (stats.weaponCatagories.Contains(WeaponCatagory.simple)){
                foreach (WeaponType weaponType in Enum.GetValues(typeof(WeaponType))){
                    if (UCombat.IsSimpleWeapon(weaponType)){
                        proficientWeapons.Add(weaponType);
                    }
                }
            }
            if (stats.weaponCatagories.Contains(WeaponCatagory.martial)){
                foreach (WeaponType weaponType in Enum.GetValues(typeof(WeaponType))){
                    if (UCombat.IsMartialWeapon(weaponType)){
                        proficientWeapons.Add(weaponType);
                    }
                }
            }
        }

        proficientWeapons.Add(WeaponType.natural);
        proficientWeapons.AddRange(stats.weaponProficiencies);

        return proficientWeapons;
    }

    public int GetProficiencyBonus(){
        return GetLevel() switch
        {
            int level when level >= 1 && level < 5 => 2,
            int level when level >= 5 && level < 9 => 3,
            int level when level >= 9 && level < 13 => 4,
            int level when level >= 13 && level < 17 => 5,
            int level when level >= 17 && level <= 20 => 6,
            _ => 0,
        };
    }

    public Dictionary<AbilityScore, int> GetAbilityScores(){

        return new Dictionary<AbilityScore, int>()
        {
        {AbilityScore.strength, stats.strength},
        {AbilityScore.dexterity, stats.dexterity},
        {AbilityScore.constitution, stats.constitution},
        {AbilityScore.intelligence, stats.intelligence},
        {AbilityScore.wisdom, stats.wisdom},
        {AbilityScore.charisma, stats.charisma},
        {AbilityScore.none, -1}
        };
    }

    public void SetCondition(Condition condition){
        Conditions.ApplyCondition(condition, this);
        currentConditions.Add(condition);
    }

    public void ClearCondition(Condition condition){
        
        // Since multiple conditions can cause the same effects (e.g disadvantage on attacks),
        // removing a condition is not as simple as removing its effects. So we remove the effects
        // and then reapply all other conditions.
        // I think this is just about the least efficient way to do it but it is unlikely that
        // a creature will have even 2 conditions at the same time so its probably not that bad. 
        
        Conditions.ClearCondition(condition, this);
        currentConditions.Remove(condition);
        foreach (Condition currentCondition in currentConditions){
            Conditions.ApplyCondition(currentCondition, this);
        }
    }   

}
