using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreatureUtils;
using CombatUtils;
using System;
using System.Linq;

public class CreatureStats : MonoBehaviour
{
    [SerializeField] Creature stats;

    public int GetMaxHP(){ return stats.maxHP;}
    public int GetAC(){ return stats.ac;}
    public int GetMovementSpeed(){ return stats.moveSpeed;}
    public CreatureSize GetSize(){ return stats.size;}
    public int GetLevel(){ return stats.level;}


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
        if (GetLevel() >= 1 && GetLevel() < 5){
            return 2;
        }
        if (GetLevel() >= 5 && GetLevel() < 9){
            return 3;
        }
        if (GetLevel() >= 9 && GetLevel() < 13){
            return 4;
        }
        if (GetLevel() >= 13 && GetLevel() < 17){
            return 5;
        }
        if (GetLevel() >= 17 && GetLevel() <= 20){
            return 6;
        }

        return 0;
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
}
