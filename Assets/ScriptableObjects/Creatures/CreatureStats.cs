using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreatureUtils;

public class CreatureStats : MonoBehaviour
{
    [SerializeField] CreatureTemplate stats;

    public int GetMaxHP(){ return stats.maxHP;}
    public int GetAC(){ return stats.ac;}
    public int GetMovementSpeed(){ return stats.moveSpeed;}
    public CreatureSize GetSize(){ return stats.size;}

    public Dictionary<AbilityScore, int> GetAbilityScores(){

        Dictionary<AbilityScore, int> abilityScores = new(){

        {AbilityScore.strength, stats.strength},
        {AbilityScore.dexterity, stats.dexterity},
        {AbilityScore.constitution, stats.constitution},
        {AbilityScore.intelligence, stats.intelligence},
        {AbilityScore.wisdom, stats.wisdom},
        {AbilityScore.charisma, stats.charisma},
        {AbilityScore.none, -1}
        };
        
        return abilityScores;
    }


}
