using System.Collections;
using System.Collections.Generic;
using CombatUtils;
using CreatureUtils;
using GameUtils;
using UnityEngine;

public class WeaponAttack : Attack, IAttack
{
    private Weapon weapon;

    public WeaponAttack(Weapon weapon){
        this.weapon = weapon;
    }

    public override DamageType GetDamageType()
    {
        return weapon.damageType[0]; 
    }

    public override int GetDamageRoll(){
        int damage = 0; 

        for (int i = 0; i < weapon.damageDie.Count; i++){
            damage += UCombat.RollDice(weapon.damageDie[i]);
        }

        return damage;
    }

    public override string GetAttackName(){
        return weapon.name;
    }

    public override AbilityScore GetDamageModifier(){
        return weapon.damageModifier;
    }

    public override int GetBonusToDamage()
    {
        return 0;
    }

    public override int GetBonusToHit()
    {
        return 0;
    }

    public override WeaponType GetWeaponType()
    {
        return weapon.weaponType;
    }

    public override int GetWeaponRange()
    {
        return weapon.weaponRange;
    }

    public override void Override(out bool overrideToHit, out bool overrideDamage)
    {
        overrideToHit = false;
        overrideDamage = false;
    }
}
