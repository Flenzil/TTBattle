using System.Collections;
using System.Collections.Generic;
using GameUtils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatEvents : MonoBehaviour
{
    private UIDocument uIDocument;
    private Button buttonWeapon;
    private Button buttonNaturalWeapon;

    private void Awake() {
        uIDocument = GetComponent<UIDocument>();
        buttonWeapon = (Button)uIDocument.rootVisualElement.Q("weapon");
        buttonWeapon.RegisterCallback<ClickEvent>(UseWeapon);
        buttonNaturalWeapon = (Button)uIDocument.rootVisualElement.Q("naturalWeapon");
        buttonNaturalWeapon.RegisterCallback<ClickEvent>(UseNaturalWeapon);

        Actions activeCreatureActions = UGame.GetActiveCreature().GetComponent<Actions>();
        List<WeaponAttack> weaponAttacks = activeCreatureActions.GetWeaponAttacks();
        List<Button> weaponButtons = new();
        for (int i = 0; i < weaponAttacks.Count; i++){
            weaponButtons.Add(new Button( () => {
                activeCreatureActions.SetActiveAttack(weaponAttacks[i]);
            }));

        }
    }

    private void OnDisable() {
        buttonWeapon.UnregisterCallback<ClickEvent>(UseWeapon);
    }

    private void UseWeapon(ClickEvent e) {
        Actions activeCreatureActions = UGame.GetActiveCreature().GetComponent<Actions>();
        activeCreatureActions.SetActiveAttack(activeCreatureActions.GetWeaponAttacks()[0]);
    }

    private void UseNaturalWeapon(ClickEvent e) {
        Actions activeCreatureActions = UGame.GetActiveCreature().GetComponent<Actions>();
        activeCreatureActions.SetActiveAttack(activeCreatureActions.GetNaturalWeaponAttacks()[0]);
    }
}
