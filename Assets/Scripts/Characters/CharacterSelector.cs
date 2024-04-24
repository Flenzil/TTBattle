using System;
using System.Collections;
using System.Collections.Generic;
using GameUtils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelector : MonoBehaviour
{
    public event EventHandler<OnActivePlayerChangedEventArgs> OnActivePlayerChanged;
    public class OnActivePlayerChangedEventArgs : EventArgs{
        public GameObject activePlayer;
    }
    Color startingColour;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Outline>().enabled = false;
        GetComponent<Outline>().OutlineWidth = 4;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetMouseButton(0)){

            if (EventSystem.current.IsPointerOverGameObject()){
                return;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool isHit = Physics.Raycast(ray, out hit);
            if (isHit){
                if (hit.transform.GameObject().layer == LayerMask.NameToLayer("Creatures")){
                    //GetComponent<CharacterPathfindingMovementHandler>().enabled = false;
                    GetComponent<Outline>().enabled = false;

                    //hit.transform.GameObject().GetComponent<CharacterPathfindingMovementHandler>().enabled = true;
                    hit.transform.GameObject().GetComponent<Outline>().enabled = true;

                    UGame.SetActiveCreature( hit.transform.GameObject());
                    TriggerActivePlayerChanged(hit.transform.GameObject());
                    
                }
            }
        }
    }

    public void TriggerActivePlayerChanged(GameObject activePlayer){
        if (OnActivePlayerChanged != null){
            OnActivePlayerChanged(this, new OnActivePlayerChangedEventArgs{ activePlayer = activePlayer});

        }
    }

    private bool IsActiveCreature(GameObject creature){
        if (creature == UGame.GetActiveCreature()) return true;
        return false;
    }

    private void OnMouseEnter() {
        GetComponent<Outline>().OutlineWidth = 4;
        GetComponent<Outline>().enabled = true;
    }

    private void OnMouseExit() {
        GetComponent<Outline>().OutlineWidth = 2;
        if (!IsActiveCreature(gameObject)) GetComponent<Outline>().enabled = false;
    }

}
