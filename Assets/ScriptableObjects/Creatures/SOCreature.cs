using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using CombatUtils;
using CreatureUtils;
using System;

[CreateAssetMenu]
public class SOCreature : ScriptableObject {

    [Header("Level")]
    public int level;

    [Header("Physical Stats")]
    public CreatureSize size;
    public int moveSpeed;
    public int ac;
    public int maxHP;

    [Header("Ability Scores")]
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    [Header("Proficiencies")]

    public List<WeaponCatagory> weaponCatagories;
    public List<WeaponType> weaponProficiencies;

    /*
    [Serializable]
    public struct Weapons{
        public List<SimpleWeapon> simpleWeapons;
        public List<MartialWeapon> martialWeapons;
    }
    */
}
