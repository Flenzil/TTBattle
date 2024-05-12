using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CreatureUtils;
using GameUtils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIWeaponsDisplay : MonoBehaviour
{
    private UIDocument uIDocument;
    private Creature activeCreature = null;
    public VisualTreeAsset weaponButtonTemplate;
    public StyleSheet weaponButtonStyle;

    private void OnEnable() {
        uIDocument = GetComponent<UIDocument>();
    }

    private void Update() {
        if (UGame.GetActiveCreature() != activeCreature) {

            uIDocument.rootVisualElement.Q("WeaponRow").Clear();
            List<Attack> attacks = UGame.GetActiveCreatureActions().GetAttacks();

            for (int i = 0; i < attacks.Count; i++){
                WeaponButton weaponButton = new WeaponButton(attacks[i], weaponButtonTemplate, weaponButtonStyle);
                uIDocument.rootVisualElement.Q("WeaponRow").Add(weaponButton.button);
            }

            uIDocument.rootVisualElement.Q("ActionsRow").Clear();
            List<System.Action> actions = GeneralActions.GetGeneralActions();

            for (int i = 0; i < actions.Count; i++){
                ActionButton actionButton = new ActionButton(actions[i], GeneralActions.GetNames()[i], weaponButtonTemplate, weaponButtonStyle);
                uIDocument.rootVisualElement.Q("ActionsRow").Add(actionButton.button);
            }

            activeCreature = UGame.GetActiveCreature();
        }
    }
}

class WeaponButton{
    
    public Button button;
    public Attack attack;

    public WeaponButton(Attack attack, VisualTreeAsset template, StyleSheet style){

        this.attack = attack;
        button = template.Instantiate().Q<Button>();
        button.text = attack.GetAttackName();
        button.styleSheets.Add(style);
        button.RegisterCallback<ClickEvent>(OnClick);
    }

    public void OnClick(ClickEvent e) {
        UGame.GetActiveCreatureActions().SetActiveAttack(attack);
    }

}

class ActionButton{
    
    public Button button;
    private Action action; 

    public ActionButton(Action action, string actionName, VisualTreeAsset template, StyleSheet style){

        this.action = action;
        button = template.Instantiate().Q<Button>();
        button.text = actionName;
        button.styleSheets.Add(style);
        button.RegisterCallback<ClickEvent>(OnClick);
    }

    public void OnClick(ClickEvent e) {
        action();
    }

}
