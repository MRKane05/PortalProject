using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class simply subscribes to a button and switches the texture on a specific material when set.
public class Toggle_TextureSwitcher : MonoBehaviour {
    public Renderer targetRenderer;
    public Material targetMaterial;

    public Texture2D TrueTexture, FalseTexture;
    public bool bIsTrue = true;

    public Material cloneMaterial;

    AudioSource ourAudio;
    public AudioClip Audio_True, Audio_False;

    void Start()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();
        //Get the target material in our renderer and replace it with a clone that we'll act upon
        Material[] targetRendererMaterials = targetRenderer.sharedMaterials;
        for (int i=0; i<targetRendererMaterials.Length; i++)
        {
            if (targetRendererMaterials[i] == targetMaterial)
            {
                cloneMaterial = new Material(targetMaterial);
                targetRendererMaterials[i] = cloneMaterial;
            }
        }
        targetRenderer.materials = targetRendererMaterials;
        SetMaterialState(bIsTrue);  //Set our initial state
    }

    public void SetMaterialState(bool bNewState)
    {
        bIsTrue = bNewState;
        cloneMaterial.SetTexture("_MainTex", bIsTrue ? TrueTexture : FalseTexture);
        if (ourAudio)
        {
            if (bNewState && Audio_True)
            {
                ourAudio.PlayOneShot(Audio_True);
            }
            if (!bNewState && Audio_False)
            {
                ourAudio.PlayOneShot(Audio_False);
            }
        }
    }
}
