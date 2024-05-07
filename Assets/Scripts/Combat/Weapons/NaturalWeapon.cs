using CombatUtils;
using CreatureUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons{

    [Serializable] 
    public struct NaturalWeapon {

        public string attackName;
        public int damageDieAmount;
        public Die damageDieSize;
        public DamageType damageType;
        public AbilityScore damageModifier;
        public int bonusToHit;
        public bool overrideToHit;
        public int bonusToDamage;
        public bool overrideDamage;

    }
}