using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using GameUtils;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHealth : MonoBehaviour
{
    public StyleSheet style;

    private UIDocument uIDocument;
    private int currentHP = -1;
    private Creature activeCreature = null;

    private void OnEnable(){
        uIDocument = GetComponent<UIDocument>();
        
    }

    private void Update(){
        if (UGame.GetActiveCreature() != activeCreature){
            if (UGame.GetActiveCreatureHealth().GetCurrentHP() != currentHP){
                currentHP = UGame.GetActiveCreatureHealth().GetCurrentHP();
                activeCreature = UGame.GetActiveCreature();

                int maxHP = UGame.GetActiveCreature().GetMaxHP();

                VisualElement root = uIDocument.rootVisualElement;

                root.Q<Label>().text = $"{currentHP}\n{maxHP}";
            }
        }
    }
}

