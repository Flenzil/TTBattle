using System;
using System.Collections;
using System.Collections.Generic;
using CreatureUtils;
using GameUtils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RightClickMenu : MonoBehaviour
{
    [SerializeField] StyleSheet buttonStyle;

    private UIDocument uIDocument;
    private VisualElement root;
    private VisualElement container;
    private Vector3 mouseScreenPosition;
    private float fadoutStart = 200f;
    private float fadoutEnd = 500f;

    void OnEnable(){
        uIDocument = GetComponent<UIDocument>();
        root = uIDocument.rootVisualElement;
        root.styleSheets.Add(buttonStyle);
    }

    // Update is called once per frame
    void Update()
    {
        //float opacity = container.resolvedStyle.opacity;

        if (container != null){
            float distanceFromBox = Vector3.Distance(Input.mousePosition, mouseScreenPosition);
            if (distanceFromBox > fadoutStart){
                container.style.opacity = 1 - (float)Math.Pow((distanceFromBox - fadoutStart)/fadoutEnd, 0.5f);
            }
        }

        //container.style.color = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a - distanceFromBox / fadoutDistance);
        if (Vector3.Distance(Input.mousePosition, mouseScreenPosition) > fadoutEnd){
            root.Clear();
        }

        if (Input.GetMouseButtonDown(1) && UGame.GetActiveCreature() != null){
            Vector3 mousePosition = UGame.GetMousePosition3D(Camera.main);
            mouseScreenPosition = Input.mousePosition;

            AddContainer(mouseScreenPosition);

            AddButton("Move Here", () => {
                PathFindingManager.Instance.SetTargetPosition(mousePosition);
                }
            );

            if (IsMouseClickOnCreature(out RaycastHit hit)){
                AddButton("Attack", ()=>{
                    CombatManager.Instance.Attack(hit.transform.GameObject(), UGame.GetActiveAttack());
                    }
                );
            }
        }
    }

    private void AddContainer(Vector3 position){
        root.Clear();
        container = new();

        container.style.position = Position.Absolute;
        container.style.top = Screen.height - position.y;
        container.style.left = position.x;
        container.style.backgroundColor= Color.white;

        root.Add(container);
    }

    private void AddButton(string buttonName, Action function){
        Button box = new Button( function );
        box.styleSheets.Add(buttonStyle);
        box.text = buttonName;
        box.style.fontSize = 30;
        container.Add(box);
    }

    private bool IsMouseClickOnCreature(out RaycastHit hit){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(ray, out hit);

        if (isHit){
            if (hit.transform.GameObject().layer == LayerMask.NameToLayer("Creatures")){
                return true;
            }
        }
        return false;
    }
}
