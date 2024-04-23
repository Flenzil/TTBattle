using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using CombatUtils;
using CreatureUtils;

[CreateAssetMenu]
public class CreatureTemplate : ScriptableObject {

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
}
