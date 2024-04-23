using UnityEngine;
using CreatureUtils;
using CombatUtils;
using GameUtils;
using Unity.VisualScripting;
using System;

public class CombatManager : MonoBehaviour
{
    [SerializeField] PathFindingManager pathfindingManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(2)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool isHit = Physics.Raycast(ray, out RaycastHit hit);
            if (isHit){
                if (hit.transform.GameObject().layer == LayerMask.NameToLayer("Creatures")){
                    PathToTarget(hit.transform.GameObject());

                    Attack(hit.transform.GameObject(), UGame.GetActiveCreature().GetComponent<Actions>().GetActiveAttack());
                }
            }
        }
    }

    private void Attack(GameObject target, Attack attack){

        AbilityScore attackAbility = attack.GetDamageModifier();

        int modifierDamage = AbilityModifier(
            UGame.GetActiveCreatureStats()
            .GetAbilityScores()[attackAbility]
            ) + attack.GetBonusToDamage();

        int modifierToHit = AbilityModifier(
            UGame.GetActiveCreatureStats()
            .GetAbilityScores()[attackAbility]
            ) + attack.GetBonusToHit();

        int diceDamage = attack.GetDamageRoll();

        int damage = diceDamage + modifierDamage;
        string weapon = attack.GetAttackName();
        string attacker = UGame.GetActiveCreature().name;
        string name = target.name;


        int dieRoll = UCombat.RollDice(Die.d20);

        if (dieRoll == 20){
            damage = diceDamage + attack.GetDamageRoll() + modifierDamage;
            target.GetComponent<Health>().Damage(damage);
            Debug.Log($"{attacker} CRITS {name} with {weapon} for {damage} damage!");
        }
        else if (dieRoll + modifierToHit >= UGame.GetCreatureStats(target).GetAC()){
            Debug.Log($"{attacker} rolls {dieRoll + modifierToHit}");
            target.GetComponent<Health>().Damage(damage);
            Debug.Log($"{attacker} hits {name} with {weapon} for {damage} damage!");
        } 
        else {
            Debug.Log($"{attacker} rolls {dieRoll + modifierToHit}");
            Debug.Log($"{attacker} misses {name} with {weapon}!");
        }
        Debug.Log($"{name} is now on {target.GetComponent<Health>().GetCurrentHP()} HP");
    }

    private int AbilityModifier(int abilityScore){
        if (abilityScore == -1){
            return 0;
        } 
        return (int)((abilityScore * 0.5f) - 5);

    }

    private void PathToTarget(GameObject target){
        pathfindingManager.SetTargetPosition(target.transform.GetChild(0).transform.position);
    }
}
