using System;
using System.Collections.Generic;
using CombatUtils;
using UnityEngine;

public class Actions : MonoBehaviour
{
    [SerializeField] List<Weapon> weapons;
    [SerializeField] List<NaturalWeapon> naturalWeapons;
    public Attack activeAttack;

    private List<WeaponAttack> WrapWeapons(List<Weapon> weapons){
        List<WeaponAttack> weaponAttacks = new();

        for (int i = 0; i < weapons.Count; i++){
            weaponAttacks.Add(new WeaponAttack(weapons[i]));
        }
        return weaponAttacks;
    }

    private List<NaturalWeaponAttack> WrapNaturalWeapons(List<NaturalWeapon> naturalWeapons){
        List<NaturalWeaponAttack> naturalWeaponAttacks = new();

        for (int i = 0; i < naturalWeapons.Count; i++){
            naturalWeaponAttacks.Add(new NaturalWeaponAttack(naturalWeapons[i]));
        }
        return naturalWeaponAttacks;
    }

    public List<WeaponAttack> GetWeaponAttacks(){
        return WrapWeapons(weapons);
    }

    public List<NaturalWeaponAttack> GetNaturalWeaponAttacks(){
        return WrapNaturalWeapons(naturalWeapons);
    }

    public List<Attack> GetAttacks(){
        List<WeaponAttack> weaponAttacks = GetWeaponAttacks();
        List<NaturalWeaponAttack> naturalWeaponAttacks = GetNaturalWeaponAttacks();

        List<Attack> attacks = new();
        attacks.AddRange(weaponAttacks);
        attacks.AddRange(naturalWeaponAttacks);
        return attacks;
    }

    public void SetActiveAttack(Attack attack){
        activeAttack = attack;
    }

    public Attack GetActiveAttack(){
        return activeAttack;
    }

}


