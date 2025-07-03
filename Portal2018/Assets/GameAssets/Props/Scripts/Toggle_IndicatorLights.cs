using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class simply subscribes to a button and switches the texture on a specific material when set.
public class Toggle_IndicatorLights : MonoBehaviour {
    public Renderer targetRenderer;
    public List<Material> cloneMaterials = new List<Material>();

    public bool bIsTrue = false;

    public Material cloneMaterial;

    void Start()
    {
        //make a copy of attached materials
        Material[] targetRendererMaterials = targetRenderer.sharedMaterials;
        for (int i = 0; i < targetRendererMaterials.Length; i++)
        {
            Material cloneMaterial = new Material(targetRendererMaterials[i]);
            cloneMaterials.Add(cloneMaterial);
            targetRendererMaterials[i] = cloneMaterial;
        }
        targetRenderer.materials = targetRendererMaterials;

        SetMaterialState(bIsTrue);  //Set our initial state
    }

    public void SetMaterialState(bool bNewState)
    {
        bIsTrue = bNewState;
        for (int i = 0; i < cloneMaterials.Count; i++)
        {
            cloneMaterials[i].SetFloat("_LerpFactor", bNewState ? 1f : 0f);
        }
    }
}
