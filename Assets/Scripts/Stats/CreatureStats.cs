using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreatureUtils;
using CombatUtils;
using System;
using System.Linq;
using UnityEngine.Rendering;
using System.Diagnostics.CodeAnalysis;

public class CreatureStats : MonoBehaviour
{
    [SerializeField] Creature stats;

    public bool hasAdvantageToHit = false;
    public bool hasDisadvantageToHit = false;
    public bool hasAdvantageToBeHit = false;
    public bool hasDisadvantageToBeHit = false;
    public List<Condition> currentConditions = new();

    private int remainingMovement;
    public int GetMaxHP(){ return stats.maxHP;}
    public int GetAC(){ return stats.ac;}
    public int GetMovementSpeed(){ return stats.moveSpeed;}
    public CreatureSize GetSize(){ return stats.size;}
    public int GetLevel(){ return stats.level;}

    void Start(){
        remainingMovement = GetMovementSpeed();
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
        Conditions.ApplyCondition(condition, GetComponent<CreatureStats>());
        currentConditions.Add(condition);
    }

    public void ClearCondition(Condition condition){
        
        // Since multiple conditions can cause the same effects (e.g disadvantage on attacks),
        // removing a condition is not as simple as removing its effects. So we remove the effects
        // and then reapply all other conditions.
        // I think this is just about the least efficient way to do it but it is unlikely that
        // a creature will have even 2 conditions at the same time so its probably not that bad. 
        
        Conditions.ClearCondition(condition, GetComponent<CreatureStats>());
        currentConditions.Remove(condition);
        foreach (Condition currentCondition in currentConditions){
            Conditions.ApplyCondition(currentCondition, GetComponent<CreatureStats>());
        }
    }   
}
