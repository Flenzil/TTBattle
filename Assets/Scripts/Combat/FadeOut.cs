using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FadeOut : MonoBehaviour
{
    //[SerializeField] Material fadeMaterial;

    private float fadeSpeed = 1f;
    private float fadeDelay = 5;
    private Renderer rend;
    private MeshFilter meshFilter;
    private Color[] colours;
    private bool isRunning = false;

    void Start(){
        rend = GetComponent<Renderer>();
        meshFilter = GetComponent<MeshFilter>();

        HDMaterial.SetSurfaceType(rend.material, true);
        rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 1);
        for (int i = 0; i < transform.childCount; i++){
            HDMaterial.SetSurfaceType(transform.GetChild(i).GetComponent<Renderer>().material, true);
            transform.GetChild(i).GetComponent<Renderer>().material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 1);
        }

        colours = colourList();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning){
            StartCoroutine(Fade());
            isRunning = true;
        }
    }

    private IEnumerator Fade(){

        yield return new WaitForSeconds(fadeDelay);

        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        foreach (Color colour in colours){

            rend.material.SetColor("_BaseColor", colour);
            for (int i = 0; i < transform.childCount; i++){
                transform.GetChild(i).GetComponent<Renderer>().material.SetColor("_BaseColor", colour);
            }
            yield return null;
        }

        Destroy(gameObject);
        yield break;
    }

    private Color[] colourList(){
        Color colour = rend.material.GetColor("_BaseColor");
        Color colourStart = new Color(colour.r, colour.g, colour.b, 1);
        Color colourEnd = new Color(colour.r, colour.g, colour.b, 0);
        Color[] lerp = new Color[(int)(1 / (fadeSpeed * Time.deltaTime))];

        for (int i = 0; i < lerp.Count() ; i++){
            lerp[i] = Color.Lerp(colourStart, colourEnd, i * (fadeSpeed * Time.deltaTime));
        }
        return lerp;
    }
}
