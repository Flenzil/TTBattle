using System.Collections;
using System.Collections.Generic;
using PathingUtils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Shatter : MonoBehaviour {

    public float pointDensity = 0.1f;
    public float explosionForce = 1000;
    public float fadeSpeed = 0.1f;
    
    [SerializeField] GameObject shatterPieces;

    private GameObject shatteredObject;

    public void Kill(int health){
        float critMultiplier = 1;
        if (CombatManager.attackRoll == 20){
            critMultiplier = 1.5f;
        }
        if (health < -10 || critMultiplier > 1){
            explosionForce += 100 * critMultiplier * -health;
        }
        shatteredObject = Instantiate(shatterPieces, transform.position, Quaternion.identity);
        shatteredObject.GetComponent<Rigidbody>().AddExplosionForce(
            explosionForce,
            shatteredObject.transform.position,
            300f
        );
        for (int i = 0; i < shatteredObject.transform.childCount; i++){
            shatteredObject.transform.GetChild(i).GetComponent<Rigidbody>().AddExplosionForce(
                explosionForce,
                shatteredObject.transform.position,
                300f
            );
        }
        Destroy(gameObject);
    }

}
