using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    private int currentHP;

    void Start(){
        currentHP = GetComponent<Creature>().GetMaxHP();
    }

    public int GetCurrentHP(){
        return currentHP;
    }

    public void Damage(int damage){
        currentHP -= damage;
    }

    public void Heal(int heal){
        currentHP += heal;
    }

}
