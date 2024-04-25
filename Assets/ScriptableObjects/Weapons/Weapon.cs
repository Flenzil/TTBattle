using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CombatUtils;
using CreatureUtils;
using UnityEngine.Assertions.Must;

[CreateAssetMenu]
public class Weapon : ScriptableObject {


    public List<Die> damageDie;
    public List<DamageType> damageType;
    public AbilityScore damageModifier;
    public WeaponType weaponType;

    public bool magical = false;

    public ScriptableObject magicalEffects;
    
    /*
    int WeaponDamage(){
        return numberOfDamageDie * damageDie;
    }

    int AdditionalWeaponDamage(){
        return additionalDamage;

    }
    */
}
