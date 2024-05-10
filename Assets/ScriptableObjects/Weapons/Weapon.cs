using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CombatUtils;
using CreatureUtils;

[CreateAssetMenu]
public class Weapon : ScriptableObject {

    public WeaponType weaponType;
    public List<Die> damageDie;
    public List<DamageType> damageType;
    public List<WeaponProperty> weaponProperties;

    [HideInInspector] public int weaponRange = 5;
    [HideInInspector] public int weaponLongRange = 0;
    [HideInInspector] public List<AbilityScore> damageModifier;

}
