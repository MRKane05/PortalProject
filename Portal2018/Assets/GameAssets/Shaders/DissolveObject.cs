using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A collection class for all the materials we might need to be dissolving
[System.Serializable]
public class SwapMaterials
{
    public Material baseMaterial;
    public Material dissolveMaterial;

    public SwapMaterials(Material baseMat, Material dissolveMat)
    {
        baseMaterial = baseMat;
        dissolveMaterial = dissolveMat;
    }
}

//A helper class to make an enemy/object dissolve by collecting all the renders it has, assigning them a controlled material
//doing a time controlled dissolve, and then removing them from the scene
public class DissolveObject : MonoBehaviour
{
    public List<SwapMaterials> swapMaterials = new List<SwapMaterials>();
    public List<SwapMaterials> swapInstances = new List<SwapMaterials>();
    public float dissolveSpeed = 0.75f;
    float dissolveAmount = 0f;
    public DestroyTrigger ourParentTrigger;

    //public Renderer[] ourRenderers;
    void populateInstances()
    {
        //We need to kickoff by making the instances we're going to use here
        foreach (SwapMaterials thisSwap in swapMaterials)
        {
            Material newBase = new Material(thisSwap.baseMaterial);
            newBase.name = newBase.name + "[inst]";
            Material newDissolve = new Material(thisSwap.dissolveMaterial);
            newDissolve.name = newDissolve.name + "[inst]";
            SwapMaterials newSwapMaterial = new SwapMaterials(newBase, newDissolve);
            swapInstances.Add(newSwapMaterial);
        }
    }

    public void Start()
    {
        populateInstances();
    }

    public void triggerDissolve()
    {
        //For the limbs we blast off
        if (swapInstances.Count != swapMaterials.Count)
        {
            populateInstances();
        }
        //We need to go through and collect all of our renderers, replace their materials with controlled materials, and then set the ticker controlling them
        List<Material> usedMaterials = new List<Material>();


        Renderer[] ourRenderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer thisRenderer in ourRenderers)
        {
            SwapRenderMaterial(thisRenderer);
        }


        Renderer objectRenderer = gameObject.GetComponent<Renderer>();
        if (objectRenderer)
        {
            SwapRenderMaterial(objectRenderer);
        }

        StartCoroutine(doDissolve());
    }

    void SwapRenderMaterial(Renderer thisRenderer)
    {
        //We have to make a copy of the shared materials and then assign it back as an array
        Material[] sharedMaterialsCopy = thisRenderer.sharedMaterials;

        for (int i = 0; i < sharedMaterialsCopy.Length; i++)
        {
            for (int m = 0; m < swapMaterials.Count; m++)
            {
                try
                {
                    if (sharedMaterialsCopy[i] != null) //because sometimes this manages to screw up
                    {
                        if (sharedMaterialsCopy[i].name == swapMaterials[m].baseMaterial.name || sharedMaterialsCopy[i] == swapMaterials[m].baseMaterial)
                        {
                            sharedMaterialsCopy[i] = swapInstances[m].dissolveMaterial;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Dissolve threw an exception: " + e.Message);
                    Debug.LogWarning("gameObject: " + gameObject.name);
                    Debug.LogWarning("SharedMaterial: " + i + "swapMaterial: " + m);
                    Debug.LogWarning("SharedMaterials Length: " + sharedMaterialsCopy.Length);
                }
            }
        }

        thisRenderer.sharedMaterials = sharedMaterialsCopy;
    }

    IEnumerator doDissolve()
    {
        while (dissolveAmount < 1.1)
        {
            yield return null;
            dissolveAmount += Time.deltaTime / dissolveSpeed;
            foreach (SwapMaterials swapMat in swapInstances)
            {
                swapMat.dissolveMaterial.SetFloat("_Amount", Mathf.Clamp(dissolveAmount, 0f, 1f));
            }
        }
        yield return null;
        if (ourParentTrigger)
        {
            ourParentTrigger.ObjectDestroyed();
        }
        Destroy(gameObject);
    }
}
