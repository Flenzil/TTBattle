using System.Collections;
using System.Collections.Generic;
using CombatUtils;
using CreatureUtils;
using UnityEngine;

public abstract class Attack
{
    public abstract string GetAttackName();
    public abstract DamageType GetDamageType();
    public abstract int GetDamageRoll();
    public abstract List<AbilityScore> GetDamageModifier();
    public abstract int GetBonusToDamage();
    public abstract int GetBonusToHit();
    public abstract WeaponType GetWeaponType();
    public abstract int GetWeaponRange();
    public abstract void Override(out bool overrideToHit, out bool overrideDamage);

}
