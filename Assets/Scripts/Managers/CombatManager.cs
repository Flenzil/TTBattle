using UnityEngine;
using CreatureUtils;
using CombatUtils;
using GameUtils;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using PathingUtils;

public class CombatManager : MonoBehaviour
{
    private static CombatManager instance;
    public static int dieRoll;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(2)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool isHit = Physics.Raycast(ray, out RaycastHit hit);
            if (isHit){
                if (hit.transform.GameObject().layer == LayerMask.NameToLayer("Creatures")){
                    Attack(hit.transform.GameObject(), UGame.GetActiveAttack());
                }
            }
        }
    }

    public static CombatManager Instance {
        get {
            if (instance == null)
                Debug.Log("Combat Manager is null");
            return instance;
        }
    }

    public void Attack(GameObject target, Attack attack){
        PathToTarget(target);

        FindHitAndDamageModifiers(attack, out int modifierToHit, out int modifierDamage);
        
        int diceDamage = attack.GetDamageRoll();

        int damage = diceDamage + modifierDamage;
        string weapon = attack.GetAttackName();
        string attacker = UGame.GetActiveCreature().name;
        string name = target.name;


        dieRoll = UCombat.RollDie(Die.d20);
        if (
            attack.GetWeaponProperties().Contains(WeaponProperty.Range)
            && Vector3.Distance(UGame.GetActiveCreature().transform.position, target.transform.position) > attack.GetWeaponRange()
            && Vector3.Distance(UGame.GetActiveCreature().transform.position, target.transform.position) < attack.GetWeaponLongRange()
        ){
            dieRoll = UCombat.RollDisadvantage(Die.d20);
        }


        if (dieRoll == 20){
            damage += attack.GetDamageRoll();
            target.GetComponent<Health>().Damage(damage);
            Debug.Log($"{attacker} CRITS {name} with {weapon} for {damage} damage!");
        }
        else if (dieRoll + modifierToHit >= UGame.GetCreatureStats(target).GetAC()){
            Debug.Log($"{attacker} rolls {dieRoll} + {modifierToHit}");
            target.GetComponent<Health>().Damage(damage);
            Debug.Log($"{attacker} hits {name} with {weapon} for {damage} damage!");
        } 
        else {
            Debug.Log($"{attacker} rolls {dieRoll} + {modifierToHit}");
            Debug.Log($"{attacker} misses {name} with {weapon}!");
        }
        Debug.Log($"{name} is now on {target.GetComponent<Health>().GetCurrentHP()} HP");

        if (target.GetComponent<Health>().GetCurrentHP() <= 0){

            UPathing.SetCreatureSpaceToUnoccupied(target);
            target.GetComponent<Shatter>().Kill(target.GetComponent<Health>().GetCurrentHP());
        }
    }


    private void FindHitAndDamageModifiers(Attack attack, out int modifierToHit, out int modifierDamage){

        AbilityScore attackAbility = FindBestAttackAbility(attack.GetDamageModifier());
        attack.Override(out bool overrideToHit, out bool overrideDamage);

        if (overrideDamage){
            modifierDamage = attack.GetBonusToDamage();
        } else{
            modifierDamage = AbilityModifier(
                UGame.GetActiveCreatureStats()
                .GetAbilityScores()[attackAbility]
                ) + attack.GetBonusToDamage();
        }

        if (overrideToHit) {
            modifierToHit = attack.GetBonusToHit();
        } else {
            modifierToHit = AbilityModifier(
                UGame.GetActiveCreatureStats()
                .GetAbilityScores()[attackAbility]
                ) + attack.GetBonusToHit();

            if (IsProfientWithWeapon()){
                modifierToHit += UGame.GetActiveCreatureStats().GetProficiencyBonus();
            }
        }   

    }

    private AbilityScore FindBestAttackAbility(List<AbilityScore> abilityScores){
        AbilityScore bestAttackAbility = abilityScores[0];

        foreach (AbilityScore abilityScore in abilityScores){
            if (
                UGame.GetActiveCreatureStats().GetAbilityScores()[abilityScore] 
                > UGame.GetActiveCreatureStats().GetAbilityScores()[bestAttackAbility]
            ){
                bestAttackAbility = abilityScore;
            }
        }
        return bestAttackAbility;
    }

    private int AbilityModifier(int abilityScore){
        if (abilityScore == -1){
            return 0;
        } 
        return (int)((abilityScore * 0.5f) - 5);
    }

    private bool IsProfientWithWeapon(){
        return UGame.GetActiveCreatureStats().GetWeaponProficiencies().Contains(UGame.GetActiveAttack().GetWeaponType());
    }

    private void PathToTarget(GameObject target){
        PathFindingManager.Instance.SetTargetPosition(target.transform.GetChild(0).transform.position, true);
    }
}
