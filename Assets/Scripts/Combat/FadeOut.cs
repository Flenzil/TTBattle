using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FadeOut : MonoBehaviour
{
    //[SerializeField] Material fadeMaterial;

    public float fadeSpeed = 0.01f;
    public float fadeDelay = 5;
    private Renderer rend;

    void Start(){
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Fade());
    }
    
    private IEnumerator Fade(){
        yield return new WaitForSeconds(fadeDelay);

        HDMaterial.SetSurfaceType(rend.material, true);

        while (true){
            Color colour = rend.material.GetColor("_BaseColor");
            Color newColour = new Color(colour.r, colour.g, colour.b, colour.a - fadeSpeed * Time.deltaTime);
            rend.material.SetColor("_BaseColor", newColour);
            if (rend.material.GetColor("_BaseColor").a <= 0){
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }

        /*
        for (float alpha = 1f; alpha >= 0; alpha -= fadeSpeed){
            colour.a = alpha;
            //rend.sharedMaterial.color = colour;
            rend.material.SetColor("_BaseColor", new Color(1,1,1,alpha));
            yield return null;
        }
        */
        //Destroy(gameObject);
    }
}

