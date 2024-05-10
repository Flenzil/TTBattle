using System.Collections.Generic;
using UnityEngine;
using CombatUtils;
using CreatureUtils;

public class NaturalWeaponAttack : Attack {


    private NaturalWeapon naturalWeapon;

    public NaturalWeaponAttack(NaturalWeapon naturalWeapon) {
        this.naturalWeapon = naturalWeapon;
    }


    public override DamageType GetDamageType()
    {
        return naturalWeapon.damageType; 
    }

    public override int GetDamageRoll(){
        int damage = 0; 

        for (int i = 0; i < naturalWeapon.damageDieAmount; i++){
            damage += UCombat.RollDie(naturalWeapon.damageDieSize);
        }

        return damage;
    }

    public override string GetAttackName()
    {
        return naturalWeapon.attackName;
    }

    public override List<AbilityScore> GetDamageModifier()
    {
        return new List<AbilityScore>()
        {
            naturalWeapon.damageModifier
        };
    }

    public override int GetBonusToHit(){
        return naturalWeapon.bonusToHit;
    }

    public override int GetBonusToDamage(){
        return naturalWeapon.bonusToDamage;
    }

    public override WeaponType GetWeaponType()
    {
        return WeaponType.natural;
    }

    public override int GetWeaponRange()
    {
        return naturalWeapon.attackRange;
    }

    public override int GetWeaponLongRange()
    {
        return 0;
    }

    public override List<WeaponProperty> GetWeaponProperties(){
        return new List<WeaponProperty>();
    }

    public override void Override(out bool overrideToHit, out bool overrideDamage)
    {
        overrideToHit = naturalWeapon.overrideToHit;
        overrideDamage = naturalWeapon.overrideDamage;
    }
}