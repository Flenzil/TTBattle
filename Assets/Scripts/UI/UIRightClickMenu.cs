using System;
using System.Collections;
using System.Collections.Generic;
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

    void OnEnable(){
        uIDocument = GetComponent<UIDocument>();
        root = uIDocument.rootVisualElement;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1)){
            Vector3 mousePosition = UGame.GetMousePosition3D(Camera.main);

            Vector3 mouseScreenPosition = Input.mousePosition;

            AddContainer(mouseScreenPosition);

            AddButton("Move Here", () => {
                Debug.Log("Moving");
                }
            );

            if (IsMouseClickOnCreature()){
                AddButton("Attack", ()=>{
                    Debug.Log("Attacking");
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

    private bool IsMouseClickOnCreature(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(ray, out RaycastHit hit);

        if (isHit){
            if (hit.transform.GameObject().layer == LayerMask.NameToLayer("Creatures")){
                return true;
            }
        }
        return false;
    }
}
